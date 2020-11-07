using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;

namespace DaAPI.Shared.Requests
{
    public static class LocalUserRequests
    {
        public static class V1
        {
            public class ResetPasswordRequest
            {
                [Required]
                [StringLength(20,MinimumLength = 5)]
                public String Password { get; set; }
            }

            public class CreateUserRequest
            {
                [Required]
                [StringLength(50, MinimumLength = 3)]
                public String Username { get; set; }

                [Required]
                [StringLength(20, MinimumLength = 5)]
                public String Password { get; set; }
            }
        }
    }
}
