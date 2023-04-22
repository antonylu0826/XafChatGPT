using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Xpo;

namespace XafChatGPT.Blazor.Server.Controllers
{
    [DomainComponent]
    [XafDisplayName("Ask ChatGPT")]
    public class ChatGptDraftTemplate : NonPersistentBaseObject
    {
        
        private string _Question;
        [XafDisplayName("Question")]
        [Size(255)]
        [ModelDefault("RowCount","10")]
        public string Question
        {
            get { return _Question; }
            set { SetPropertyValue<string>(ref _Question, value); }
        }


    }
}
