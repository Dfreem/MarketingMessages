using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketingMessages.Shared;

public static class BrowserStorageKeys
{
    public const string EDITOR_CONTENT = "EditorContent";
}
public static class SettingNameKeys
{
    public const string LogRolloverInterval = "Log Rollover Interval";
    public const string DeletedCampaignsAffectStatistics = "Deleted Campaign Affects Statistics";
    public const string EmailBatchSize = "Email Sending Batch Size";
    public const string EmailBatchInterval = "Email Batch Interval";
    public const string EmailWorkerTimerInterval = "Email Worker Timer Interval";
    public const string EmailQueueConcurrencyLimit = "Concurrently Running Email Queue Limit";
}

public static class CampaignType
{
    public const string TEST = "TEST";
    public const string RECURRING = "RECURRING";

    public const string DEFAULT = "DEFAULT";
}

public static class WebhookEventType
{
    public const string PROCESSED = "processed";
    public const string Open = "open";
    public const string CLICKED = "clicked";
}
/// <summary>
/// Contains string constants for all supported document.execCommand() commands.
/// Note: execCommand is deprecated, but still used in some editors.
/// </summary>
public static class ExecCommandNames
{
    public const string BackColor = "backColor";
    public const string Bold = "bold";
    public const string Copy = "copy";
    public const string CreateLink = "createLink";
    public const string Cut = "cut";
    public const string DecreaseFontSize = "decreaseFontSize";
    public const string Delete = "delete";
    public const string FontName = "fontName";
    public const string FontSize = "fontSize";
    public const string ForeColor = "foreColor";
    public const string FormatBlock = "formatBlock";
    public const string ForwardDelete = "forwardDelete";
    public const string Heading = "heading";
    public const string HiliteColor = "hiliteColor";
    public const string IncreaseFontSize = "increaseFontSize";
    public const string Indent = "indent";
    public const string InsertBrOnReturn = "insertBrOnReturn";
    public const string InsertHorizontalRule = "insertHorizontalRule";
    public const string InsertHTML = "insertHTML";
    public const string InsertImage = "insertImage";
    public const string InsertOrderedList = "insertOrderedList";
    public const string InsertUnorderedList = "insertUnorderedList";
    public const string InsertParagraph = "insertParagraph";
    public const string InsertText = "insertText";
    public const string Italic = "italic";
    public const string JustifyCenter = "justifyCenter";
    public const string JustifyFull = "justifyFull";
    public const string JustifyLeft = "justifyLeft";
    public const string JustifyRight = "justifyRight";
    public const string Outdent = "outdent";
    public const string Paste = "paste";
    public const string Redo = "redo";
    public const string RemoveFormat = "removeFormat";
    public const string SelectAll = "selectAll";
    public const string StrikeThrough = "strikeThrough";
    public const string Subscript = "subscript";
    public const string Superscript = "superscript";
    public const string Underline = "underline";
    public const string Undo = "undo";
    public const string Unlink = "unlink";
    public const string UseCSS = "useCSS"; // legacy, not always supported
}
