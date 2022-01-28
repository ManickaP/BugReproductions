// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Net;

//using var listener = new HttpEventListener();

var subscription = DiagnosticListener.AllListeners.Subscribe(new CallbackObserver<DiagnosticListener>(listener =>
{
    if (listener.Name == "HttpHandlerDiagnosticListener")
    {
        listener.Subscribe(new CallbackObserver<KeyValuePair<string, object?>>(value =>
        {
            Console.WriteLine("NEXT:");
            Console.WriteLine($"Event: {value.Key} [{value.Value?.GetType().FullName}]{value.Value}");
            if (Activity.Current != null) {
                Console.WriteLine($"Activity: {Activity.Current.Id}");
            }
            Console.WriteLine();
        }));
    }
}));


// If you want the see the order of activities created, add ActivityListener.
ActivitySource.AddActivityListener(new ActivityListener()
{
    ShouldListenTo = (activitySource) => true,
    ActivityStarted = activity => Console.WriteLine($"ACTIVITY: Start {activity.DisplayName}={activity.Id}"),
    ActivityStopped = activity => Console.WriteLine($"ACTIVITY: Stop {activity.DisplayName}={activity.Id}")
});

//Console.WriteLine("Hello, World!");

// Set up headers propagator for this client.
var client = new HttpClient(new SocketsHttpHandler() {
    // -> Turns off activity creation as well as header injection
    // ActivityHeadersPropagator = null

    // -> Activity gets created but Request-Id header is not injected
    // ActivityHeadersPropagator = DistributedContextPropagator.CreateNoOutputPropagator()

    // -> Activity gets created, Request-Id header gets injected and contains "root" activity id
    ActivityHeadersPropagator = DistributedContextPropagator.CreatePassThroughPropagator()

    // -> Activity gets created, Request-Id header gets injected and contains "parent" activity id
    // ActivityHeadersPropagator = new SkipHttpClientActivityPropagator()

    // -> Activity gets created, Request-Id header gets injected and contains "System.Net.Http.HttpRequestOut" activity id
    // Same as not setting ActivityHeadersPropagator at all.
    // ActivityHeadersPropagator = DistributedContextPropagator.CreateDefaultPropagator()
});

// Set up activities, at least two layers to show all the differences.
using Activity root = new Activity("root");
root.SetIdFormat(ActivityIdFormat.W3C);
root.Start();
/*using Activity parent = new Activity("parent");
parent.SetIdFormat(ActivityIdFormat.Hierarchical);
parent.Start();*/

var request = new HttpRequestMessage(HttpMethod.Get, "https://www.microsoft.com");

using var response = await client.SendAsync(request);
Console.WriteLine($"Request: {request}"); // Print the request to see the injected header

public sealed class SkipHttpClientActivityPropagator : DistributedContextPropagator
{
    private readonly DistributedContextPropagator _originalPropagator = Current;

    public override IReadOnlyCollection<string> Fields => _originalPropagator.Fields;

    public override void Inject(Activity? activity, object? carrier, PropagatorSetterCallback? setter)
    {
        if (activity?.OperationName == "System.Net.Http.HttpRequestOut")
        {
            activity = activity.Parent;
        }

        _originalPropagator.Inject(activity, carrier, setter);
    }

    public override void ExtractTraceIdAndState(object? carrier, PropagatorGetterCallback? getter, out string? traceId, out string? traceState) =>
        _originalPropagator.ExtractTraceIdAndState(carrier, getter, out traceId, out traceState);

    public override IEnumerable<KeyValuePair<string, string?>>? ExtractBaggage(object? carrier, PropagatorGetterCallback? getter) =>
        _originalPropagator.ExtractBaggage(carrier, getter);
}

public class CallbackObserver<T> : IObserver<T>
{
    public CallbackObserver(Action<T> callback) { _callback = callback; }
    public void OnCompleted() { Console.WriteLine("COMPLETED"); }
    public void OnError(Exception error) { Console.WriteLine("ERROR: " + error); }
    public void OnNext(T value) { _callback(value); }

    private Action<T> _callback;
}