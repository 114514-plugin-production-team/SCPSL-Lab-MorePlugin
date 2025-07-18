using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LabMorePlugins.Interfaces
{
    public interface IAbility
    {
        string Name { get; }

        string Description { get; }

        int KeyId { get; }

        KeyCode KeyCode { get; }

        float Cooldown { get; }

        void Register();

        void Unregister();
    }
}
