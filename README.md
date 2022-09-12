# HeartlandContactList
A console app, with concurrent users, that stores persistent data to a file.

## Design description / Patterns
### The app work in the following fashion:
1. The Program.cs class is responsible for creating service used for Dependency injection and starting the app. The app  uses the *Microsoft.Extensions.Dependency injection* Dependency to create and configure Services to be Injected through the app. The ContactRepository is a scoped service which, is created once per client request; this was chosen because the does not carry a state that is needed longer than the request. The DbContext is a singleton, which are created the first time they're requested and last the lifetime of the Process; this was chosen because the DbContext is required to be a single source of truth. The ContactsListApp is a transient, which are created each time they're requested from the service container; this would give each Process (or user in this case) their own version of the app.

2. The ContactiListApp.cs class is responsible for the UI and logic loop of the app. Here the contactRepository is constructor-injected into the class, so it can be configured (see step for 3 details). This class uses a loop to prompt the user to enter Contact information and add them to the contactRepository. The properties of the Contact entity are looped through via Reflection and propertyInfo and data attributes or used to aid in prompting the use, creating the entity to be store, and validating the user input.

3. The ContactRepository.cs class is responsible for encapsulating the logic required to access data sources. The DbContext is constructorinjected, and is used to update the datasource. Additional event functions were added to this case to simplify abstraction. This class contains 2 EventWaitHandle which are used to achieve "real time" interprocess communication, used to synchronize the contact count in each of concurrent version of the console (each users app). These EventWaitHandles are tied to an EventHandler which calls back to the ContactListApp.cs class to update the user's console. A separate thread is created in each process to monitor the EventWaitHandles and trigger the event (this is the EventThread.cs class). In general (with an AutoRest EventResetMode), a EventWaitHandle will only release 1 waiting Process, and therefore trigger only 1 console to be updated. To work around this, timers were added to trigger all waiting EventWaitHandles. This is not the most ideal method, and given more time, it would be best to investigate a better solution for this situation; **Creating a service/microservice or using a messaging queue (such as RabbitMQ or MQTT) to handle the real time update is more ideal and scalable**

4. The DbContext.cs class is used to mimic a database. This class contains functions to update the file via json serialization (update), and deserialization(to read the file and get the count). A named Mutex is used in the class to protect the file from being written to while a process or thread is reading it, and vice versa. The class inherits the IDisposable interface, so this Mutex can be properly handled when the Datasource is no longer needed. Serialization and deserialization are preformed in separate tasks (thread), to allow the main thread to be responsive while the these process are preformed.

5. The Contact.cs class acts as a model for the type of Entity to be stored in the datasource. Data Annotation are added to the properties of this class to aid in validation, and provide a "screen display name" used in the UI.

6. The DbContext.cs, in this case, acts as a mock database context.

7. All files in the *contracts* directory are used to provide interfaces for Dependency Injection.

### Design Patterns Used:
1. Dependency Injection: This allow for the creation of the "mock database" and ContactRepository to be preformed in a single location, and used where needed throughout the app in a manner that provides consistent data and function.

2. Reflection: this simplified the UI presentation and validation loop, and allow for a smaller code base. It also allows additional fields to be added to a model without needing to add much to the UI loop.

### Other Techniques Used:
1. Event-Driven Architecture: Allowed for "real time" updates, and accuracy with the contact count.
 
2. Resource locking (Mutex): Protected shared resource (the shared file).

3. Asynchronous Programming: Allowed for resource intensive processes to be preformed in the background (not blocking the main thread).

## Explanation of Future Enhancements:
### The command-line will be replaced with a GUI.
The UI has been encapsulated to the ContactListApp.cs class, which could be converted to a Winforms, WPF, or even Web client. With Dependency Injection implemented and this encapsulation, this class could essentially be replaced with whatever desired UI the stakeholders would like.

### The application will need to support capturing additional information about a person, such as email address.
To implement this, simple additions need to be made to the Contacts.cs class. In the case of the email address, an emailAddress property could be added with a *PhoneAttribute* type data annotation added to it.

### The application will need to support more complex rules/constraints such as maximum length and proper formatting on input.
These are, again, simple data annotation additions to the Contact.cs class.

### The rules could vary depending on the customer needs so the rules need to be configurable.
Custom rules could be implemented using the data annotation Validtor class.

### The data will need to be persisted to a remote database.
This would require a small change to the Program.cs class; adding AddDbContext to the services, and possibly implementing an Object/Relational Mapping (O/RM) framework such as EntityFramework Core.

### The application will also need to communicate through a middle-tier in order to avoid exposing the database publicly.
Essentially, if the UI (ContactListApp.cs) class was removed in favor of some sort of controller (such as one used in a MVC architecture) and a client application, and, the Datasource.cs class was replaced with a database; this application would become the middle-tier.
