
namespace Sitecore.Support.Shell.Applications.ContentEditor
{
  using Sitecore;
  using Sitecore.Configuration;
  using Sitecore.Data;
  using Sitecore.Diagnostics;
  using Sitecore.Globalization;
  using Sitecore.Shell.Applications.ContentEditor.RichTextEditor;
  using Sitecore.Text;
  using Sitecore.Web;
  using Sitecore.Web.UI.HtmlControls;
  using Sitecore.Web.UI.Sheer;
  using Sitecore.WordOCX;
  using System;
  using System.Web;

  /// <summary>Represents a Rich Text field.</summary>
  public class RichText : Frame
  {
    /// <summary>The handle.</summary>
    private string handle;

    /// <summary>The item version.</summary>
    private string itemVersion;

    /// <summary>The set value on pre render.</summary>
    private bool setValueOnPreRender;

    /// <summary>
    /// Gets or sets the field ID.
    /// </summary>
    /// <value>The field ID.</value>
    public string FieldID
    {
      get
      {
        return base.GetViewStateString("FieldID");
      }
      set
      {
        Assert.ArgumentNotNull(value, "value");
        base.SetViewStateString("FieldID", value);
      }
    }

    /// <summary>
    /// Gets or sets the item ID.
    /// </summary>
    /// <value>The item ID.</value>
    public string ItemID
    {
      get
      {
        return base.GetViewStateString("ItemID");
      }
      set
      {
        Assert.ArgumentNotNull(value, "value");
        base.SetViewStateString("ItemID", value);
      }
    }

    /// <summary>
    /// Gets or sets the item language.
    /// </summary>
    /// <value>The item language.</value>
    public string ItemLanguage
    {
      get
      {
        return base.GetViewStateString("ItemLanguage");
      }
      set
      {
        Assert.ArgumentNotNull(value, "value");
        base.SetViewStateString("ItemLanguage", value);
      }
    }

    /// <summary>
    /// Gets or sets the item version.
    /// </summary>
    /// <value>The item version.</value>
    public string ItemVersion
    {
      get
      {
        return this.itemVersion;
      }
      set
      {
        Assert.ArgumentNotNull(value, "value");
        this.itemVersion = value;
      }
    }

    /// <summary>
    /// Gets or sets the source.
    /// </summary>
    /// <value>The source.</value>
    public string Source
    {
      get
      {
        return base.GetViewStateString("Source");
      }
      set
      {
        Assert.ArgumentNotNull(value, "value");
        base.SetViewStateString("Source", value);
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is tracking modified.
    /// </summary>
    /// <value><c>true</c> if  this instance is tracking modified; otherwise, <c>false</c>.</value>
    public bool TrackModified
    {
      get
      {
        return base.GetViewStateBool("TrackModified", true);
      }
      set
      {
        base.SetViewStateBool("TrackModified", value, true);
      }
    }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <value>The value.</value>
    public override string Value
    {
      get
      {
        WordFieldValue wordFieldValue = WordFieldValue.Parse(base.Value);
        if (wordFieldValue.BlobId != Sitecore.Data.ID.Null)
        {
          return wordFieldValue.GetHtmlWithStyles();
        }
        return base.Value;
      }
      set
      {
        base.Value = value;
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:Sitecore.Shell.Applications.ContentEditor.RichText" /> class.
    /// </summary>
    public RichText()
    {
      this.Class = "scContentControlHtml";
      base.Activation = true;
      base.AllowTransparency = false;
      base.Attributes["tabindex"] = "-1";
    }

    /// <summary>Handles the message.</summary>
    /// <param name="message">The message.</param>
    public override void HandleMessage(Message message)
    {
      Assert.ArgumentNotNull(message, "message");
      base.HandleMessage(message);
      if (message["id"] == this.ID)
      {
        string name = message.Name;
        if (!(name == "richtext:edit"))
        {
          if (!(name == "richtext:edithtml"))
          {
            if (name == "richtext:fix")
            {
              Sitecore.Context.ClientPage.Start(this, "Fix");
            }
          }
          else
          {
            Sitecore.Context.ClientPage.Start(this, "EditHtml");
          }
        }
        else
        {
          Sitecore.Context.ClientPage.Start(this, "EditText");
        }
      }
    }

    /// <summary>Edits the text.</summary>
    /// <param name="args">The args.</param>
    protected void EditHtml(ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");
      if (!this.Disabled)
      {
        if (args.IsPostBack)
        {
          if (args.Result != null && args.Result != "undefined")
          {
            this.UpdateHtml(args);
          }
        }
        else
        {
          UrlString urlString = new UrlString("/sitecore/shell/-/xaml/Sitecore.Shell.Applications.ContentEditor.Dialogs.EditHtml.aspx");
          UrlHandle urlHandle = new UrlHandle();
          string text = this.Value;
          if (text == "__#!$No value$!#__")
          {
            text = string.Empty;
          }

          // Begin fix for bug 228036
          if (text.Equals("&nbsp;"))
          {
            text = string.Empty;
          }
          // End fix for bug 228036

          urlHandle["html"] = text;
          urlHandle.Add(urlString);
          SheerResponse.ShowModalDialog(new ModalDialogOptions(urlString.ToString())
          {
            Width = "1000",
            Height = "500",
            Response = true,
            Header = Translate.Text("HTML Editor")
          });
          args.WaitForPostBack();
        }
      }
    }

    /// <summary>Edits the text.</summary>
    /// <param name="args">The args.</param>
    protected void EditText(ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");
      if (!this.Disabled)
      {
        if (args.IsPostBack)
        {
          if (args.Result != null && args.Result != "undefined")
          {
            this.UpdateHtml(args);
          }
        }
        else
        {
          RichTextEditorUrl richTextEditorUrl = new RichTextEditorUrl
          {
            Conversion = RichTextEditorUrl.HtmlConversion.DoNotConvert,
            Disabled = this.Disabled,
            FieldID = this.FieldID,
            ID = this.ID,
            ItemID = this.ItemID,
            Language = this.ItemLanguage,
            Mode = string.Empty,
            ShowInFrameBasedDialog = true,
            Source = this.Source,
            Url = "/sitecore/shell/Controls/Rich Text Editor/EditorPage.aspx",
            Value = this.Value,
            Version = this.ItemVersion
          };
          UrlString url = richTextEditorUrl.GetUrl();
          this.handle = richTextEditorUrl.Handle;
          SheerResponse.ShowModalDialog(new ModalDialogOptions(url.ToString())
          {
            Width = "1200",
            Height = "700px",
            Response = true,
            Header = Translate.Text("Rich Text Editor")
          });
          args.WaitForPostBack();
        }
      }
    }

    /// <summary>Fixes the text.</summary>
    /// <param name="args">The args.</param>
    protected void Fix(ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");
      if (!this.Disabled)
      {
        if (args.IsPostBack)
        {
          if (args.Result != null && args.Result != "undefined")
          {
            this.UpdateHtml(args);
          }
        }
        else
        {
          UrlString urlString = new UrlString("/sitecore/shell/-/xaml/Sitecore.Shell.Applications.ContentEditor.Dialogs.FixHtml.aspx");
          UrlHandle urlHandle = new UrlHandle();
          string text = this.Value;
          if (text == "__#!$No value$!#__")
          {
            text = string.Empty;
          }
          urlHandle["html"] = text;
          urlHandle.Add(urlString);
          SheerResponse.ShowModalDialog(urlString.ToString(), "800px", "500px", string.Empty, true);
          args.WaitForPostBack();
        }
      }
    }

    /// <summary>Loads the post data.</summary>
    /// <param name="value">The value.</param>
    /// <returns>The load post data.</returns>
    protected override bool LoadPostData(string value)
    {
      Assert.ArgumentNotNull(value, "value");
      if (value != this.Value)
      {
        this.Value = value;
        return true;
      }
      return false;
    }

    /// <summary>Raises the <see cref="E:System.Web.UI.Control.Load"></see> event.</summary>
    /// <param name="e">The <see cref="T:System.EventArgs"></see> object that contains the event data.</param>
    protected override void OnLoad(EventArgs e)
    {
      Assert.ArgumentNotNull(e, "e");
      base.OnLoad(e);
      if (!Sitecore.Context.ClientPage.IsEvent)
      {
        RichTextEditorUrl richTextEditorUrl = new RichTextEditorUrl
        {
          Conversion = RichTextEditorUrl.HtmlConversion.DoNotConvert,
          Disabled = this.Disabled,
          FieldID = this.FieldID,
          ID = this.ID,
          ItemID = this.ItemID,
          Language = this.ItemLanguage,
          Mode = "ContentEditor",
          ShowInFrameBasedDialog = true,
          Source = this.Source,
          Url = string.Empty,
          Value = this.Value,
          Version = this.ItemVersion
        };
        UrlString url = richTextEditorUrl.GetUrl();
        this.handle = richTextEditorUrl.Handle;
        this.setValueOnPreRender = true;
        base.SourceUri = url.ToString();
      }
    }

    /// <summary>Raises the <see cref="E:System.Web.UI.Control.PreRender"></see> event.</summary>
    /// <param name="e">An <see cref="T:System.EventArgs"></see> object that contains the event data. </param>
    protected override void OnPreRender(EventArgs e)
    {
      Assert.ArgumentNotNull(e, "e");
      base.OnPreRender(e);
      if (this.setValueOnPreRender)
      {
        HttpContext.Current.Session[this.handle] = this.Value;
      }
      base.ServerProperties["ItemLanguage"] = base.ServerProperties["ItemLanguage"];
      base.ServerProperties["Source"] = base.ServerProperties["Source"];
      base.ServerProperties["FieldID"] = base.ServerProperties["FieldID"];
    }

    /// <summary>Sets the modified.</summary>
    protected virtual void SetModified()
    {
      if (this.TrackModified)
      {
        Sitecore.Context.ClientPage.Modified = true;
        SheerResponse.Eval("scContent.startValidators()");
      }
    }

    /// <summary>
    /// Updates the HTML.
    /// </summary>
    /// <param name="args">The arguments.</param>
    protected virtual void UpdateHtml(ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");
      string text = args.Result;
      if (text == "__#!$No value$!#__")
      {
        text = string.Empty;
      }
      text = this.ProcessValidateScripts(text);
      if (text != this.Value)
      {
        this.SetModified();
      }
      SheerResponse.Eval("scForm.browser.getControl('" + this.ID + "').contentWindow.scSetText(" + StringUtil.EscapeJavascriptString(text) + ")");
      SheerResponse.Eval("scContent.startValidators()");
    }

    /// <summary>
    /// Processes the validate scripts.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>Result of the value.</returns>
    protected string ProcessValidateScripts(string value)
    {
      if (Settings.HtmlEditor.RemoveScripts)
      {
        value = WebUtil.RemoveAllScripts(value);
      }
      return value;
    }
  }
}