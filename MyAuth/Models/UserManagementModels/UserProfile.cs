﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyAuth.Models.UserManagementModels
{
    public class UserProfile
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }
}