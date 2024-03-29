﻿using DbTools.Entities.Abstract;

namespace UserRegistrationAPI.API.DataContracts
{
    public class Address : Entity
    {
        public string City { get; set; }
        public string Street { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
    }
}
