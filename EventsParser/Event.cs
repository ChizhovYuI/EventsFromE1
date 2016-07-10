using System;

namespace EventsInEkb
{
    public class Event
    {
        public Event(DateTime date, string place, string address, string name)
        {
            Date = date;
            Place = place;
            Address = address;
            Name = name;
        }

        public DateTime Date { get; private set; } 
        public string Place { get; private set; }
        public string Address { get; private set; }
        public string Name { get; private set; }
        public override string ToString()
        {
            return String.Format("Date: {0}, Place: {1}, Address: {2}, Name: {3}", Date, Place, Address, Name);
        }

    }
}