using DevExpress.Blazor.RichEdit;
using DevExpress.DataAccess.Native.Web;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Office.Blazor.Components.Models;
using DevExpress.ExpressApp.Office.Blazor.Editors;
using DevExpress.ExpressApp.Office.Blazor.Editors.Adapters;
using DevExpress.ExpressApp.Templates;
using Microsoft.AspNetCore.Components;
using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;
using XafChatGPT.Module.BusinessObjects;

namespace XafChatGPT.Blazor.Server.Controllers
{
    public class AskGptViewController : ObjectViewController<DetailView, Announcement>
    {
        PopupWindowShowAction popupWindow;
        public AskGptViewController()
        {
            popupWindow = new PopupWindowShowAction()
            {
                Id = "MyFirstObject_popupWindow_detail",
                Caption = "Ask ChatGPT",
                ImageName = "BO_MyDetails",
                PaintStyle = ActionItemPaintStyle.CaptionAndImage,
                Category = "Edit",
            };
            popupWindow.Execute += PopupWindow_Execute;
            popupWindow.CustomizePopupWindowParams += PopupWindow_CustomizePopupWindowParams;
            Actions.Add(popupWindow);
        }        

        private void PopupWindow_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            var nos = (NonPersistentObjectSpace)e.Application.CreateObjectSpace(typeof(ChatGptDraftTemplate));
            nos.PopulateAdditionalObjectSpaces(Application);
            e.DialogController.SaveOnAccept = false;
            var data = nos.CreateObject<ChatGptDraftTemplate>();
            e.View = e.Application.CreateDetailView(nos, data);
        }

        private async void PopupWindow_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            var popupObject = e.PopupWindowViewCurrentObject as ChatGptDraftTemplate;

            var openAiService = new OpenAIService(new OpenAiOptions()
            {
                ApiKey = "your open ai api key"
            });

            var messages = new List<ChatMessage>
            {
                new(StaticValues.ChatMessageRoles.User, popupObject.Question)
            };
            try
            {
                var completionResult = openAiService.ChatCompletion.CreateCompletionAsStream(new ChatCompletionCreateRequest
                {
                    Messages = messages,
                    MaxTokens = 1000,
                    Model = Models.ChatGpt3_5Turbo
                });

                var content = "";
                await foreach (var completion in completionResult)
                {
                    if (completion.Successful)
                    {
                        var c = completion.Choices.First().Message.Content;
                        if (c != null)
                        {
                            await Selection.ActiveSubDocument.AddTextAsync(Selection.CaretPosition, c);
                            Selection = new Selection(Selection.ActiveSubDocument, Selection.CaretPosition + c.Length);
                        }
                        
                        content += c;
                    }
                    else
                    {
                        if (completion.Error == null)
                        {
                            throw new Exception("Unknown Error");
                        }                        
                        Console.WriteLine($"{completion.Error.Code}: {completion.Error.Message}");
                    }
                }

                messages.Add(new(StaticValues.ChatMessageRoles.Assistant, content));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }


        }


        #region RichText

        protected override void OnActivated()
        {
            base.OnActivated();
            foreach (RichTextPropertyEditor editor in View.GetItems<RichTextPropertyEditor>())
            {
                if (editor.Control != null)
                {
                    /*_richEditModel = ((RichTextEditorComponentAdapter)editor.Control).ComponentModel;*/
                    CustomizeRichEditComponentModel(((RichTextEditorComponentAdapter)editor.Control).ComponentModel);
                }
                else
                {
                    editor.ControlCreated += Editor_ControlCreated;
                }
            }
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            foreach (RichTextPropertyEditor editor in View.GetItems<RichTextPropertyEditor>())
            {
                editor.ControlCreated -= Editor_ControlCreated;
            }
        }

        private void Editor_ControlCreated(object sender, EventArgs e)
        {
            CustomizeRichEditComponentModel(((RichTextEditorComponentAdapter)((RichTextPropertyEditor)sender).Control).ComponentModel);
        }

        private Selection Selection { get; set; }

        private void CustomizeRichEditComponentModel(DxRichEditModel richEditModel)
        {
            //
            richEditModel.Selection = Selection;
            
            richEditModel.SelectionChanged = EventCallback.Factory.Create<Selection>(this, (selection) => Selection = selection);
        }
        #endregion
    }
}
