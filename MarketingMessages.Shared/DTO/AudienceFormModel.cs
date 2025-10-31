using MarketingMessages.Shared.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MarketingMessages.Shared.DTO;


public class AudienceFormModel
{
    public string Title { get; set; } = "Untitled";
    public int SegmentId { get; set; }
    public SuppressionGroupModel? UnsubscribeGroup { get; set; }

    public ConditionalInclusion AnyOrAll { get; set; }

    public List<SegmentFormRow> Rows { get; set; } = [];
}

public class SegmentFormRow
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SegmentCategory { get; set; } = "";
    public string AnyOrAll { get; set; } = ConditionalInclusion.Any.ToString();
    public string? Operator { get; set; }
    public List<string> QueryParams { get; set; } = [];
}
public enum SegmentCategory
{
    None,
    //Tags,
    //Address,
    State,
    Profession,
    //TextField,
    //DateField,
    //ContactRating,
    //EmailClient,
    //InfoChanged,
    //Language,
    //PhoneNumber,
    //PredictedAgeRange,
    //PredictedGender,
    //VIPStatus,
    ZipCode,
    //EmailDateAdded,
    //SmsDateAdded,
    //SignupSource,
    //AutomationActivity,
    //EmailEngagement,
    //EmailInteraction,
    //EmailSubscriptionStatus,
    //SmsEngagement,
    //SmsInteraction,
    //SmsSubscriptionStatus,
    //EcommerceActivity,
    //LikelihoodToPurchaseAgain,
    //CustomerLifetimeValue,
    //ConversationActivity,
    //WebsiteEngagement,
    //LandingPageActivity,
    //PollSurveyActivity,
    //SurveyMonkeyActivity
}
public static class SegmentCategoryExtensions
{
    public static string[] GetOperators(this SegmentCategory category) => category switch
    {
        //SegmentCategory.Address => Enum.GetNames<AddressOperator>(),
        SegmentCategory.ZipCode => Enum.GetNames<ZipCodeOperator>().Where(s => s != "None").ToArray(),
        SegmentCategory.State => Enum.GetNames<StateAbbreviation>().Where(s => s != "None").ToArray(),
        //SegmentCategory.EmailDateAdded => Enum.GetNames<EmailDateAddedOperator>(),
        //SegmentCategory.EmailInteraction => Enum.GetNames<EmailInteractionOperator>(),
        SegmentCategory.Profession => Enum.GetNames<ProfessionOperator>().Where(s => s != "None").ToArray(),
        //SegmentCategory.Tags => Enum.GetNames<TagOperator>(),
        //SegmentCategory.TextField => Enum.GetNames<TextFieldOperator>(),
        //SegmentCategory.DateField => Enum.GetNames<DateFieldOperator>(),
        //SegmentCategory.ContactRating => Enum.GetNames<ContactRatingOperator>(),
        //SegmentCategory.EmailClient => Enum.GetNames<EmailClientOperator>(),
        //SegmentCategory.Language => Enum.GetNames<LanguageOperator>(),
        //SegmentCategory.PhoneNumber => Enum.GetNames<PhoneNumberOperator>(),
        //SegmentCategory.PredictedAgeRange => Enum.GetNames<PredictedAgeRange>(), // behaves like operator set
        //SegmentCategory.PredictedGender => Enum.GetNames<PredictedGender>(),     // behaves like operator set
        //SegmentCategory.VIPStatus => Enum.GetNames<VIPStatusOperator>(),
        //SegmentCategory.SmsDateAdded => Enum.GetNames<SmsDateAddedOperator>(),
        //SegmentCategory.SignupSource => Enum.GetNames<SignupSourceOperator>(),
        //SegmentCategory.EmailEngagement => Enum.GetNames<EmailEngagementLevel>(), // acts like operator
        //SegmentCategory.EmailSubscriptionStatus => Enum.GetNames<EmailSubscriptionStatus>(),
        //SegmentCategory.LandingPageActivity => Enum.GetNames<LandingPageActivityOperator>(),
        //SegmentCategory.PollSurveyActivity => Enum.GetNames<PollSurveyActivityOperator>(),
        _ => []
    };
}

public enum ProfessionOperator
{
    Is,
    IsNot,
    Contains
}

public enum EmailInteractionOperator
{
    Opened,
    DidNotOpen,
    Clicked,
    DidNotClick,
    WasSent,
    WasNotSent
}
public enum EmailSubscriptionStatus
{
    Subscribed,
    Unsubscribed,
    NonSubscribed,
    Cleaned
}

public enum SegmentFormType
{
    None,
    ContactProperties
}
public enum ConditionalInclusion
{
    All,
    Any
}
public enum ZipCodeOperator
{
    Is,
    IsNot,
    //IsWithinDistance
}

public enum TagOperator
{
    Is,
    IsNot
}

public enum AddressOperator
{
    Is,
    IsNot,
    Contains,
    DoesNotContain
    //IsWithinDistance
}
public enum TextFieldOperator
{
    Contains,
    DoesNotContain,
    StartsWith,
    EndsWith,
    //IsBlank,
    //IsNotBlank,
    Is,
    IsNot
}

public enum DateFieldOperator
{
    IsOnDate,
    IsNotOnDate,
    IsBeforeDate,
    IsAfterDate,
    IsBetween,
    MonthIs,
    DayIs,
    IsBlank,
    IsNotBlank
}
public enum ContactRatingOperator
{
    Is,
    IsGreaterThan,
    IsLessThan
}
public enum EmailClientOperator
{
    Is,
    IsNot
}

public enum EmailClient
{
    AppleMail,
    Gmail,
    Outlook,
    Yahoo,
    Thunderbird,
    Other
}
public enum LanguageOperator
{
    Is,
    IsNot
}
public enum PhoneNumberOperator
{
    IsBlank,
    IsNotBlank
}

public enum PredictedAgeRange
{
    Under21,
    Between21And30,
    Between31And40,
    Between41And50,
    Between51And60,
    Over60,
    Unknown
}

public enum PredictedGender
{
    Male,
    Female,
    Unknown
}
public enum VIPStatusOperator
{
    IsVIP,
    IsNotVIP
}
public enum EmailDateAddedOperator
{
    IsOnDate,
    IsBeforeDate,
    IsAfterDate,
    IsBetween,
    TheLastXDays
}
public enum SmsDateAddedOperator
{
    IsOnDate,
    IsBeforeDate,
    IsAfterDate,
    IsBetween,
    TheLastXDays
}
public enum SignupSourceOperator
{
    Is,
    IsNot
}

public enum SignupSource
{
    API,
    AdminAdd,
    HostedSignupForm,
    Integration,
    Import,
    Unknown
}
public enum EmailEngagementLevel
{
    New,
    Rarely,
    Sometimes,
    Often
}
public enum LandingPageActivityOperator
{
    SignedUpOn,
    DidNotSignUpOn
}
public enum PollSurveyActivityOperator
{
    Responded,
    DidNotRespond
}

