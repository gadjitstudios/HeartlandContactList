using System.IO;
using System.Linq;
using System;
using System.Text.Json;
using System.Collections;
using System.Threading.Tasks;
using System.Threading;

namespace Heartland.data{
    public class Datasource<T> : IDisposable{
        private readonly Mutex mut;
        const string MUTEX_NAME = "HearlandContactList";
        const int MUTEX_TIMEOUT = 5000;
        private string filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
            ,$"Heartland\\{typeof(T).Name}.data"
        );

        public Datasource()
        {
            var doesMutexExist = Mutex.TryOpenExisting(MUTEX_NAME, out mut);
            if(!doesMutexExist){
                mut = new Mutex(true, MUTEX_NAME);
                mut.ReleaseMutex();
            }
                
            
            if(!Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        }

        ///<summary>
        /// Serializes values to file
        ///</summary>
        ///<param name="values">IQueryable<T> of values to serialize</param>
        private async Task Serialize(IQueryable<T> values){
            await Task.Run(() =>{
                try{
                    mut.WaitOne(MUTEX_TIMEOUT);
                    var options = new JsonSerializerOptions();
                    options.WriteIndented = true;
                    var jsonString = JsonSerializer.Serialize(values, options);
                    File.WriteAllText(filePath, jsonString);
                    mut.ReleaseMutex();
                } catch(Exception e) {
                    mut.ReleaseMutex();
                } 
            });
            
        }

        ///<summary>
        /// Deserialize file
        ///</summary>
        ///<returns>IQueryable<T> of values to from a file</returns>
        private async Task<IQueryable<T>> Deserialize(){
            if(File.Exists(filePath)){
                return await Task.Run(() =>{
                    try{
                        mut.WaitOne(MUTEX_TIMEOUT);
                        var jsonString = File.ReadAllText(filePath);
                        var _values = JsonSerializer.Deserialize<T[]>(jsonString);
                        mut.ReleaseMutex();
                        if(_values != null)
                            return _values.AsQueryable();
                    } catch(Exception e) {
                        mut.ReleaseMutex();
                    }
                    return null;
                });
                
            }
            return null;
        }

        ///<summary>
        /// Adds a entity to the file
        ///</summary>
        ///<param name="t">Entity to add to the file (of type T)</param>
        ///<returns>Task</returns>
        public async Task Add(T t){
            var values = Enumerable.Empty<T>().Append(t).AsQueryable();
            IQueryable<T> _values = await Deserialize();
            if(_values != null)
                values = values.Concat(_values); 
            await Serialize(values);
        }

        ///<summary>
        /// Gets the number of entities in the file
        ///</summary>
        ///<returns>number of entities in the file</returns>
        public async Task<int> GetCount(){
            var values = await Deserialize();
            return values != null? values.Count() : 0;
        }

        public void Dispose()
        {
            mut.ReleaseMutex();
            mut.Close();
        }
    }
}