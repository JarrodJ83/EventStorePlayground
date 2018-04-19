using System;

namespace Producer.Events
{
    public class NewCustomerRegistered
    {
        public string Name { get; }
        public DateTime DateOfBirth { get; }

        public NewCustomerRegistered(string name, DateTime dateOfBirth)
        {
            Name = name;
            DateOfBirth = dateOfBirth;
        }
    }
}