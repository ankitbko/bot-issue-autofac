using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

namespace bot_issue_autofac
{
    class Program
    {
        static void Main(string[] args)
        {
            Program p = new Program();
            p.StartBot();
            Console.ReadLine();
        }

        private async void StartBot()
        {
            try
            {
                Activity activity = new Activity()
                {
                    From = new ChannelAccount { Id = Guid.NewGuid().ToString() },
                    Conversation = new ConversationAccount { Id = Guid.NewGuid().ToString() },
                    Recipient = new ChannelAccount { Id = "Bot" },
                    ServiceUrl = "https://skype.botframework.com",
                    ChannelId = "skype",
                    Text="Hi"
                };

                //using (var scope = Conversation
                //    .Container.BeginLifetimeScope(DialogModule.LifetimeScopeTag))
                using (var scope = Conversation
                    .Container.BeginLifetimeScope(DialogModule.LifetimeScopeTag, Configure)) // This overload is the issue
                {
                    scope.Resolve<IMessageActivity>
                        (TypedParameter.From((IMessageActivity)activity));
                    DialogModule_MakeRoot.Register
                        (scope, () => new DummyDialog());
                    var postToBot = scope.Resolve<IPostToBot>();
                    await postToBot.PostAsync(activity, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private void Configure(ContainerBuilder builder)
        {
            // You may even comment this out. It will still throw error.
            builder.Register(c => new BotToUserQueue(c.Resolve<IMessageActivity>(), new Queue<IMessageActivity>()))
                .As<IBotToUser>()
                .InstancePerLifetimeScope();
        }
    }

    // Trying to mimic LuisDialog. You can use LuisDialog in place also.
    [Serializable]
    internal class DummyDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait<object>(MessageRecieved);
        }

        private async Task MessageRecieved(IDialogContext context, IAwaitable<object> result)
        {
            context.Wait<object>(MessageRecieved);
        }
    }
}
