using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;

namespace XafChatGPT.Module.BusinessObjects
{
    [NavigationItem("ChatGPT Test")]
    public class Announcement : BaseObject
    {
        public Announcement(Session session) : base(session) { }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }

        string fName;
        [XafDisplayName("Title")]
        public string Name
        {
            get { return fName; }
            set { SetPropertyValue(nameof(Name), ref fName, value); }
        }

        private byte[] fWordContent;
        [Size(SizeAttribute.Unlimited)]
        [EditorAlias(DevExpress.ExpressApp.Editors.EditorAliases.RichTextPropertyEditor)]
        [XafDisplayName("Content")]
        [DetailViewLayout("Word 編輯器", 500)]
        public byte[] WordContent
        {
            get { return fWordContent; }
            set { SetPropertyValue(nameof(WordContent), ref fWordContent, value); }
        }
    }
}
