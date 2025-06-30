using MapGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabMorePlugins.Enums
{
    public class ZoneNum
    {
        public static Dictionary<MapGeneration.FacilityZone, string> Zone = new Dictionary<MapGeneration.FacilityZone, string>()
        {
            {
                FacilityZone.Surface,
                "地表"
            },
            {
                FacilityZone.Other,
                "其他"
            },
            {
                FacilityZone.HeavyContainment,
                "重收"
            },
            {
                FacilityZone.LightContainment,
                "清收"
            },
            {
                FacilityZone.Entrance,
                "入口处"
            }
        };
    }
}
