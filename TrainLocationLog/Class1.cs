using System;
using System.Collections.Generic;
using System.Text;

namespace TrainLocationLog
{

    public class HF_car_type
    {
        public string dt { get; set; }
        public Type[] type { get; set; }
        public class Type
        {
            public string style_line { get; set; }
            public string code { get; set; }
            public string style_hrm { get; set; }
            public string flg { get; set; }
            public string name { get; set; }
        }
    }



    public class HF_station
    {
        public Ikisaki[] ikisaki { get; set; }
        public string dt { get; set; }

        public class Ikisaki
        {
            public string name { get; set; }
            public string code { get; set; }
            public string yomi { get; set; }
            public string eiji { get; set; }
        }
    }


}
