using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Threading.Tasks;
using Heartland.contracts;
using Heartland.models;
using Heartland.repositories;
using Heartland.util;

namespace Heartland{
    public class ContactsListApp{
        private  IContactRepository _ContactRepository;
        
        public ContactsListApp(IContactRepository contactRepository)
        {
            // Initialize
            _ContactRepository = contactRepository;
            _ContactRepository.CountAddedEvent += HandleUpdateCount;
            _ContactRepository.StartListeningForUpdateCount();
        }
        public async Task Run()
        {
            // Setup Console
            Console.Clear();
            Console.WriteLine();
            await UpdateCount();

            Contact _contact = new Contact();
            do{
                ClearConsole();
                foreach(var prop in typeof(Contact).GetProperties()){
                    bool _IsValidProp = true;
                    string _optionalString = "";

                    // Get Data Annotations
                    var _name = prop.GetCustomAttribute<DisplayAttribute>().Name;
                    var _optional = prop.GetCustomAttribute<RequiredAttribute>();
                    var _phone = prop.GetCustomAttribute<PhoneAttribute>();
                    var _customAttributes = prop.GetCustomAttributes();
                    do{
                        _IsValidProp = true;

                        // Pompt user
                        _optionalString = _optional == null? "[OPTIONAL]" : "";
                        Console.WriteLine($"Please enter a value for the contact's {_name} {_optionalString}");

                        // Get user input
                        var _value = Console.ReadLine();

                        // Validate
                        foreach(var attr in _customAttributes){
                            switch(attr.GetType().Name){
                                case "RequiredAttribute":
                                    if(!_optional.IsValid(_value)){
                                        _IsValidProp = false;
                                        Console.WriteLine($"{_name} is a required field");
                                    }
                                    break;
                                case "PhoneAttribute":
                                    if(!_phone.IsValid(_value)){
                                        _IsValidProp = false;
                                        Console.WriteLine($"{_name} is requires a valid telephone number format");
                                    }
                                    break;
                            }
                        }
                            
                        if(_IsValidProp)
                            prop.SetValue(_contact, _value);
                        
                    }while(!_IsValidProp);

                    ClearConsole();
                }

                // Update contact list and count
                _ContactRepository.Add(_contact);

                Console.WriteLine("Enter {ESC} quit, {ANY OTHER KEY} add new contact");
            } while(Console.ReadKey().Key != ConsoleKey.Escape);
        }
        private void HandleUpdateCount(object sender, EventArgs args){
            UpdateCount().Wait();
        }

        ///<summary>
        /// Gets count and updates the top line (used to display the count) of the console
        ///</summary>
        public async Task UpdateCount(){
            var _count = await _ContactRepository.GetCount();
            UpdateCount(_count);
        }

        ///<summary>
        /// Clears all but the top line (used to display the count) of the console
        ///</summary>
        public void ClearConsole(){
            for(var i = Console.CursorTop; i > 0; i--){
                Console.SetCursorPosition(0, i);
                Console.Write(new string(' ', Console.WindowWidth)); 
                Console.SetCursorPosition(0, i);
            }
        }

        ///<summary>
        /// Updates the top line (used to display the count) of the console
        ///</summary>
        public void UpdateCount(int count){
            var curPosTop = Console.CursorTop;
            var curPosLeft = Console.CursorLeft;
            Console.SetCursorPosition(0, 0);
            Console.Write($"Contact count: {count}");
            Console.SetCursorPosition(curPosLeft, curPosTop);
        }
    }
}