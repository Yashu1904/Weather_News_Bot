﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using AdaptiveCards;

namespace trialweatherbot
{
    
        public class BingNews
        {
            public string readLink { get; set; }
            public int totalEstimatedMatches { get; set; }
            public NewsResult[] value { get; set; }
       
    }

        public class NewsResult
        {
            public string name { get; set; }
            public string url { get; set; }
            public NewsImage image { get; set; }
            public string description { get; set; }
            public Provider[] provider { get; set; }
            public DateTime datePublished { get; set; }
        }

        public class NewsImage
        {

        public Thumbnail thumbnail { get; set; }
    }

        public class Thumbnail
        {
            public string contentUrl { get; set; }
            public int width { get; set; }
            public int height { get; set; }
        }

        public class Provider
        {
            public string name { get; set; }
        }
   
        
    
}
