using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SKYPE4COMLib;

namespace Cosmos.Tools.SkypeBot
{
    /// <summary>
    /// Represents the skype bot.
    /// </summary>
    public class SkypeBot : IDisposable
    {
        /// <summary>
        /// Represents the Cosmos-Dev chat room.
        /// </summary>
        public const string Cosmos_Dev = "VXO9PL3t2HgKSEPgytWb7EM0OaRRmfhWLKd8v-2SV_6m43_Bli5m6mFNNQxjv8TyTb1GpaXZfz_5VFYS17NvKSLLICj4MWZslQ";

        /// <summary>
        /// Occurs when a message is received.
        /// </summary>
        public event EventHandler<SkypeMessageEventArgs> MessageReceived;

        private Chat _chat;
        private Skype _skype;

        /// <summary>
        /// Creates a new skype bot using the specified blob.
        /// </summary>
        /// <param name="blob"></param>
        public SkypeBot(string blob)
        {
            _skype = new SKYPE4COMLib.SkypeClass();
            _skype.Attach(5, true);
            _skype.MessageStatus += new _ISkypeEvents_MessageStatusEventHandler(_skype_MessageStatus);
            _chat = _skype.CreateChatUsingBlob(blob);
        }

        void _skype_MessageStatus(ChatMessage pMessage, TChatMessageStatus Status)
        {
            // Only react to our thread.
            if (pMessage.Chat.Blob == _chat.Blob &&
                (Status == TChatMessageStatus.cmsReceived ||
                Status == TChatMessageStatus.cmsSent))
            {
                SkypeMessage msg = new SkypeMessage();
                msg.Body = pMessage.Body;
                msg.From = pMessage.Sender.FullName;
                msg.Sent = DateTime.Now.ToUniversalTime();

                OnMessageReceived(msg);
            }
        }

        /// <summary>
        /// Occurs when a message is received.
        /// </summary>
        /// <param name="msg"></param>
        protected virtual void OnMessageReceived(SkypeMessage msg)
        {
            if (MessageReceived != null)
                MessageReceived(this, new SkypeMessageEventArgs(msg));
        }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="message"></param>
        protected void SendMessage(string message)
        {
            _chat.SendMessage(message);
        }

        #region IDisposable Members

        public void Dispose()
        {
            MessageReceived = null;
        }

        #endregion
    }
}
