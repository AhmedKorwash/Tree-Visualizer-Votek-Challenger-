using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VotekChallengeWeb.Models
{
    public class TreeModels
    {
        public TreeModels()
        {
            Childs = new List<TreeModels>();
        }
        public string id
        {
            get;
            set;
        }
        public string Title
        {
            get;
            set;
        }
        public string Previous_Id
        {
            get;
            set;
        }
        public string Next_Id
        {
            get;
            set;
        }
        public List<TreeModels> Childs { get; set; }
    }
}