// See https://aka.ms/new-console-template for more information
using TimeLoggingApp.Domain;

var eventSubject = await new MicrosoftGraphWrapper().GetFirstEventOfDay();
Console.WriteLine(eventSubject);
