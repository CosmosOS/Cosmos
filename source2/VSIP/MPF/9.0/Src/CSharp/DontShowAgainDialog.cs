/// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;

namespace Microsoft.VisualStudio.Project
{
	/// <summary>
	/// Defines a genric don't show again dilaog.
	/// </summary>
	internal partial class DontShowAgainDialog : Form
	{
		#region constants
		/// <summary>
		/// Defines the General subkey under VS hive for the CurrentUser.
		/// </summary>
		private static string GeneralSubKey = "General";
		#endregion

		#region fields
		/// <summary>
		/// Defines the bitmap to be drawn
		/// </summary>
		private Bitmap bitmap;

		/// <summary>
		/// The associated service provider
		/// </summary>
		private IServiceProvider serviceProvider;

		/// <summary>
		/// The help topic associated.
		/// </summary>
		private string helpTopic;

		/// <summary>
		/// The value of the don't show again check box
		/// </summary>
		private bool dontShowAgainValue;
		#endregion

		#region constructors
		/// <summary>
		/// Overloaded constructor
		/// </summary>
		/// <param name="serviceProvider">The associated service provider.</param>
		/// <param name="messageText">Thetext to be shown on the dialog</param>
		/// <param name="helpTopic">The associated help topic</param>
		/// <param name="button">The default button</param>
		internal DontShowAgainDialog(IServiceProvider serviceProvider, string messageText, string helpTopic, DefaultButton button)
		{
			if(serviceProvider == null)
			{
				throw new ArgumentNullException("serviceProvider");
			}

			this.serviceProvider = serviceProvider;
			this.InitializeComponent();

			if(button == DefaultButton.OK)
			{
				this.AcceptButton = this.okButton;
			}
			else
			{
				this.AcceptButton = this.cancelButton;
			}


			this.SetupComponents(messageText, helpTopic);
		}
		#endregion

		#region properties
		/// <summary>
		/// The value of the dont' show again checkbox before the dialog is closed.
		/// </summary>
		internal bool DontShowAgainValue
		{
			get
			{
				return this.dontShowAgainValue;
			}
		}
		#endregion

		#region methods
		/// <summary>
		/// Shows help for the help topic.
		/// </summary>
		protected virtual void ShowHelp()
		{
			Microsoft.VisualStudio.VSHelp.Help help = this.serviceProvider.GetService(typeof(Microsoft.VisualStudio.VSHelp.Help)) as Microsoft.VisualStudio.VSHelp.Help;

			if(help != null)
			{
				help.DisplayTopicFromF1Keyword(this.helpTopic);
			}
		}

		/// <summary>
		/// Launches a DontShowAgainDialog if it is needed.
		/// </summary>
		/// <param name="serviceProvider">An associated serviceprovider.</param>
		/// <param name="messageText">The text the dilaog box will contain.</param>
		/// <param name="helpTopic">The associated help topic.</param>
		/// <param name="button">The default button.</param>
		/// <param name="registryKey">The registry key that serves for persisting the not show again value.</param>
		/// <returns>A Dialog result.</returns>
		internal static DialogResult LaunchDontShowAgainDialog(IServiceProvider serviceProvider, string messageText, string helpTopic, DefaultButton button, string registryKey)
		{
			if(String.IsNullOrEmpty(registryKey))
			{
				throw new ArgumentException(SR.GetString(SR.ParameterCannotBeNullOrEmpty, CultureInfo.CurrentUICulture), "registryKey");
			}

			DialogResult result = DialogResult.OK;

			bool dontShowAgain = ReadDontShowAgainValue(registryKey);

			if(!dontShowAgain)
			{
				DontShowAgainDialog dialog = new DontShowAgainDialog(serviceProvider, messageText, helpTopic, button);
				result = dialog.ShowDialog();

				// Now write to the registry the value.
				if(dialog.DontShowAgainValue)
				{
					WriteDontShowAgainValue(registryKey, 1);
				}
			}

			return result;
		}

		/// <summary>
		/// Reads a boolean value specifying whether to show or not show the dialog
		/// </summary>
		/// <param name="registryKey">The key containing the value.</param>
		/// <returns>The value read. If the value cannot be read false is returned.</returns>
		internal static bool ReadDontShowAgainValue(string registryKey)
		{
			bool dontShowAgain = false;
			using(RegistryKey root = VSRegistry.RegistryRoot(__VsLocalRegistryType.RegType_UserSettings))
			{
				if(root != null)
				{
					using(RegistryKey key = root.OpenSubKey(GeneralSubKey))
					{
						int value = (int)key.GetValue(registryKey, 0);
						dontShowAgain = (value != 0);
					}
				}
			}

			return dontShowAgain;
		}

		/// <summary>
		/// Writes a value 1 in the registrykey and as aresult the dont show again dialog will not be launched.
		/// </summary>
		/// <param name="registryKey">The key to write to.</param>
		/// <param name="value">The value to write.</param>
		internal static void WriteDontShowAgainValue(string registryKey, int value)
		{
			using(RegistryKey root = VSRegistry.RegistryRoot(__VsLocalRegistryType.RegType_UserSettings))
			{
				if(root != null)
				{
					using(RegistryKey key = root.OpenSubKey(GeneralSubKey, true))
					{
						key.SetValue(registryKey, value, RegistryValueKind.DWord);
					}
				}
			}
		}

		/// <summary>
		/// Shows the dialog if possible hosted by the IUIService.
		/// </summary>
		/// <returns>A DialogResult</returns>
		internal new DialogResult ShowDialog()
		{
			Debug.Assert(this.serviceProvider != null, "The service provider should not be null at this time");
			IUIService uiService = this.serviceProvider.GetService(typeof(IUIService)) as IUIService;
			if(uiService == null)
			{
				return this.ShowDialog();
			}

			return uiService.ShowDialog(this);
		}

		/// <summary>
		/// Defines the event delegate when help is requested.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="hlpevent"></param>
		private void OnHelpRequested(object sender, HelpEventArgs hlpevent)
		{
			if(String.IsNullOrEmpty(this.helpTopic))
			{
				return;
			}

			this.ShowHelp();
			hlpevent.Handled = true;
		}

		/// <summary>
		/// Defines the delegate that responds to the help button clicked event.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">An instance of canceleventargs </param>
		private void OnHelpButtonClicked(object sender, CancelEventArgs e)
		{
			if(String.IsNullOrEmpty(this.helpTopic))
			{
				return;
			}

			e.Cancel = true;
			this.ShowHelp();
		}

		/// <summary>
		/// Called when the dialog box is repainted.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The associated paint event args.</param>
		private void OnPaint(object sender, PaintEventArgs e)
		{
			e.Graphics.DrawImage(this.bitmap, new Point(7, this.messageText.Location.Y));
		}


		/// <summary>
		/// Sets up the components that are not done through teh Initialize components.
		/// </summary>
		/// <param name="helpTopicParam">The associated help topic</param>
		/// <param name="messageTextParam">The message to show on the dilaog.</param>
		private void SetupComponents(string messageTextParam, string helpTopicParam)
		{
			// Compute the Distance to the bottom of the dialog 
			int distanceToBottom = this.Size.Height - this.cancelButton.Location.Y;

			// The Y end coordinate of the messageText before it assigned its value.
			int deltaY = this.messageText.Location.Y + this.messageText.Size.Height;

			// Set the maximum size as the CancelButtonEndX - MessageTextStartX. This way it wil never pass by the button.
			this.messageText.MaximumSize = new Size(this.cancelButton.Location.X + this.cancelButton.Size.Width - this.messageText.Location.X, 0);
			this.messageText.Text = messageTextParam;

			// How much it has changed?
			deltaY = this.messageText.Size.Height - deltaY;
			this.AdjustSizesVertically(deltaY, distanceToBottom);

			if(String.IsNullOrEmpty(helpTopicParam))
			{
				this.HelpButton = false;
			}
			else
			{
				this.helpTopic = helpTopicParam;
			}

			// Create the system icon that will be drawn on the dialog page.
			Icon icon = new Icon(SystemIcons.Exclamation, 40, 40);

			// Call ToBitmap to convert it.
			this.bitmap = icon.ToBitmap();

			this.CenterToScreen();
		}

		/// <summary>
		/// Handles the cancel button clicked event.
		/// </summary>
		/// <param name="sender">The sender of teh event.</param>
		/// <param name="e">The event args associated to teh event.</param>
		private void OnCancelButtonClicked(object sender, EventArgs e)
		{
			this.dontShowAgainValue = this.dontShowAgain.Checked;
			this.DialogResult = DialogResult.Cancel;
		}

		/// <summary>
		/// Handles the cancel button clicked event.
		/// </summary>
		/// <param name="sender">The sender of teh event.</param>
		/// <param name="e">The event args associated to teh event.</param>
		private void OnOKButtonClicked(object sender, EventArgs e)
		{
			this.dontShowAgainValue = this.dontShowAgain.Checked;
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		/// <summary>
		///  Moves controls vertically because of a vertical change in the messagetext.
		/// </summary>
		private void AdjustSizesVertically(int deltaY, int distanceToBottom)
		{
			// Move the checkbox to its new location determined by the height the label.
			this.dontShowAgain.Location = new Point(this.dontShowAgain.Location.X, this.dontShowAgain.Location.Y + deltaY);

			// Move the buttons to their new location; The X coordinate is fixed.
			int newSizeY = this.cancelButton.Location.Y + deltaY;
			this.cancelButton.Location = new Point(this.cancelButton.Location.X, newSizeY);

			newSizeY = this.okButton.Location.Y + deltaY;
			this.okButton.Location = new Point(this.okButton.Location.X, newSizeY);

			// Now resize the dialog itself.
			this.Size = new Size(this.Size.Width, this.cancelButton.Location.Y + distanceToBottom);
		}
		#endregion

		#region nested types
		/// <summary>
		/// Defines which button to serve as the default button.
		/// </summary>
		internal enum DefaultButton
		{
			OK,
			Cancel,
		}
		#endregion
	}
}