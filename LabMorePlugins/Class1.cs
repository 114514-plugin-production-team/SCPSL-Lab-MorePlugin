using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabMorePlugins
{
    public class Class1
    {
        public bool IsEnabled { get; set; } = true;
        [Description("SCP杀人类获得多少积分")]
        public int SCPKillPeople { get; set; } = 20;
        [Description("人类杀人类获得多少积分")]
        public int PeopleKillPeople { get; set; } = 20;
        [Description("人类杀SCP获得多少积分")]
        public int PeopleKillSCP { get; set; } = 20;
        [Description("每多少积分升一等级")]
        public int LevelCanUp { get; set; } = 250;
        [Description("刷新九尾狐时的广播")]
        public string SpawnNTF { get; set; } = "九尾大军来了";
        [Description("刷新混沌时的广播")]
        public string SpawnCI { get; set; } = "混沌来了";
        [Description("是否启动SCP106一击必杀")]
        public bool SCP106Pock {  get; set; } = true;
    }
}
