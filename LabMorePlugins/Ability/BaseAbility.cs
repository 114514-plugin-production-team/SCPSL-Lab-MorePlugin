using LabApi.Features.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UserSettings.ServerSpecific;
using UserSettings.ServerSpecific.Examples;

namespace LabMorePlugins.Ability
{
    public abstract class BaseAbility
    {
        private static readonly Dictionary<int, BaseAbility> _abilitiesById = new Dictionary<int, BaseAbility>();
        private static readonly Dictionary<string, BaseAbility> _abilitiesByName = new Dictionary<string, BaseAbility>();
        public abstract int ID { get; set; }
        public abstract string Name { get; set; }
        public abstract KeyCode KeyCode { get; set; }
        public abstract string Description { get; set; }
        public virtual string KeyCodeName { get; set; }
        public virtual string KeyCodeDescription { get; set; }
        public abstract AbilityType Type { get; set; }
        public enum AbilityType
        {
            Toggle = 1,
            Hold = 0
        }
        private SSPagesExample.SettingsPage _abilityPage;
        private Dictionary<ReferenceHub, int> _lastSentPages;
        private SSDropdownSetting _pageSelectorDropdown;
        private ServerSpecificSettingBase[] _pinnedSection;
        protected virtual void Reg()
        {
            if (!_abilitiesById.ContainsKey(ID))
                _abilitiesById.Add(ID, this);

            if (!_abilitiesByName.ContainsKey(Name))
                _abilitiesByName.Add(Name, this);

            // 创建专属配置页面
            _abilityPage = new SSPagesExample.SettingsPage(
                Name, // 页面名称 = 技能名称
                new ServerSpecificSettingBase[]
                {
                    new SSGroupHeader(Name, false, Description),
                    new SSKeybindSetting(ID, KeyCodeName, KeyCode, true, KeyCodeDescription),
                    new SSTwoButtonsSetting((int)Type, "触发模式", "长按", "按一下", false, "选择技能触发方式"),
                    // 可以在这里添加更多专属配置项...
                }
            );

            // 初始化页面管理
            _lastSentPages = new Dictionary<ReferenceHub, int>();

            // 创建技能选择器（下拉菜单）
            string[] pageNames = _abilitiesById.Values.Select(ability => ability.Name).ToArray();
            _pinnedSection = new ServerSpecificSettingBase[]
            {
                _pageSelectorDropdown = new SSDropdownSetting(
                    null,
                    "选择技能配置",
                    pageNames,
                    0,
                    SSDropdownSetting.DropdownEntryType.HybridLoop,
                    "选择要配置的技能"
                )
            };

            // 注册所有设置项
            var allSettings = new List<ServerSpecificSettingBase>(_pinnedSection);
            allSettings.AddRange(_abilityPage.OwnEntries);
            ServerSpecificSettingsSync.DefinedSettings = allSettings.ToArray();

            // 监听设置变化
            ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnServerOnSettingValueReceived;
            ServerSpecificSettingsSync.SendToAll();
        }
        protected virtual void UnReg()
        {
            _abilitiesById.Remove(ID);
            _abilitiesByName.Remove(Name);
            ServerSpecificSettingsSync.ServerOnSettingValueReceived -= OnServerOnSettingValueReceived;
        }
        private void OnServerOnSettingValueReceived(ReferenceHub hub, ServerSpecificSettingBase setting)
        {
            if (setting is SSDropdownSetting dropdown && dropdown.SettingId == _pageSelectorDropdown.SettingId)
            {
                // 切换技能配置页面
                ServerSendSettingsPage(hub, dropdown.SyncSelectionIndexValidated);
                return;
            }

            // 如果修改的是当前技能的按键绑定
            if (setting.SettingId == ID)
            {
                Player player = Player.Get(hub);
                if (player != null)
                    EventForPlayer(player, setting as SSKeybindSetting);
            }
        }
        private void ServerSendSettingsPage(ReferenceHub hub, int pageIndex)
        {
            if (_lastSentPages.TryGetValue(hub, out int lastPage) && lastPage == pageIndex)
                return;

            _lastSentPages[hub] = pageIndex;
            ServerSpecificSettingsSync.SendToPlayer(hub, _abilityPage.CombinedEntries, null);
        }
        protected virtual void EventForPlayer(Player player, SSKeybindSetting keybind)
        {

        }
        public static BaseAbility GetAbility(object identifier)
        {
            if (identifier is int id && _abilitiesById.TryGetValue(id, out var ability))
                return ability;
            if (identifier is string name && _abilitiesByName.TryGetValue(name, out ability))
                return ability;

            throw new KeyNotFoundException($"Ability not found: {identifier}");
        }

        public static bool TryGet(object identifier, out BaseAbility ability)
        {
            ability = null;
            if (identifier is int id)
                return _abilitiesById.TryGetValue(id, out ability);
            if (identifier is string name)
                return _abilitiesByName.TryGetValue(name, out ability);

            return false;
        }
    }
}
