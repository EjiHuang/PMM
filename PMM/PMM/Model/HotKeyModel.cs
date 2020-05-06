using DevExpress.Mvvm;
using InputInterceptorNS;

namespace TOS_TW_TOOL.Models
{
    public class HotKeyModel : ViewModelBase
    {
        /// <summary>
        /// HP药水快捷键
        /// </summary>
        public string Key4Skill
        {
            get => GetProperty(() => Key4Skill);
            set => SetProperty(() => Key4Skill, value);
        }

        /// <summary>
        /// 启动HP热键监控
        /// </summary>
        public bool Key4SkillEnable
        {
            get => GetProperty(() => Key4SkillEnable);
            set => SetProperty(() => Key4SkillEnable, value);
        }

        /// <summary>
        /// HP热键框焦点
        /// </summary>
        public bool HotKeyTextBoxFocusable4Skill
        {
            get => GetProperty(() => HotKeyTextBoxFocusable4Skill);
            set => SetProperty(() => HotKeyTextBoxFocusable4Skill, value);
        }
    }
}
