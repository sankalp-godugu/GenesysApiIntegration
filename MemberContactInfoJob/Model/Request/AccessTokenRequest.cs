﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemberContactInfoJob.Model.Request
{
    public class AccessTokenRequest
    {
        public string Grant_Type => "client_credentials";
        public string client_id => "eade7e44-e2e1-467e-97aa-6253a5b3051f";
        public string client_secret => "6wuNbhnIX39P3w06p79Lp6LcW1F-uqUK5ZYRYyuI9Ys";
    }
}