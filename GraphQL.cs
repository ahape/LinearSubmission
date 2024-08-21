using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace LinearSubmission.GraphQL;

public class IssueArguments
{
    public string? Id { get; set; }
}

public class IssueQuery : MutationOrQuery<IssueArguments>
{
    protected override string GetQueryString() => Flatten(@"
query Issue($id: String!) {
  issue(id: $id) {
    number,
    identifier,
    url
  }
}");
    protected override string GetOperationName() => "Issue";
}

public class IssueCreateArguments
{
    public IssueCreateInput? Input { get; set; }
}

public class IssueCreateInput
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? TeamId { get; set; }
    public string[]? LabelIds { get; set; } = [];
}

public class IssueCreateMutation : MutationOrQuery<IssueCreateArguments>
{
    protected override string GetQueryString() => Flatten(@"
mutation IssueCreate($input: IssueCreateInput!) {
  issueCreate(input: $input) {
    success
    issue {
      id
      title
    }
  }
}");
    protected override string GetOperationName() => "IssueCreate";
}

public class AttachmentLinkUrlArguments
{
    public string? Url { get; set; }
    public string? IssueId { get; set; }
}

public class AttachmentLinkUrlMutation : MutationOrQuery<AttachmentLinkUrlArguments>
{
    protected override string GetQueryString() => Flatten(@"
mutation AttachmentLinkURL($url: String!, $issueId: String!) {
  attachmentLinkURL(url: $url, issueId: $issueId) {
    attachment {
      url
    }
  }
}");
    protected override string GetOperationName() => "AttachmentLinkURL";
}

public abstract class MutationOrQuery<T>
{
    public string Query => GetQueryString();
    public string OperationName => GetOperationName();
    public T? Variables { get; init; }

    protected abstract string GetQueryString();
    protected abstract string GetOperationName();

    protected static string Flatten(string multilineTemplateString)
    {
        // Return it all as a single line
        return Regex.Replace(multilineTemplateString.Trim(), @"\s+", " ");
    }

    private static JsonSerializerOptions serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, serializerOptions);
    }
}