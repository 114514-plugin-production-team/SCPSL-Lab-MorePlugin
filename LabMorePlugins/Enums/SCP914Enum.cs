using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabMorePlugins.Enums
{
    public class SCP914Enum
    {
        public static Dictionary<Scp914.Scp914KnobSetting, string> SCP914KnobSetting = new Dictionary<Scp914.Scp914KnobSetting, string>()
        {
            {
                Scp914.Scp914KnobSetting.OneToOne,
                "一比一"
            },
            {
                Scp914.Scp914KnobSetting.Fine,
                "精加工"
            },
            {
                Scp914.Scp914KnobSetting.VeryFine,
                "超精"
            },
            {
                Scp914.Scp914KnobSetting.Coarse,
                "粗加工"
            },
            {
                Scp914.Scp914KnobSetting.Rough,
                "粗糙加工"
            }
        };
    }
}
