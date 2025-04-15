## Adding validators for Command and Query objects

* Required (i.e. non-nullable) properties are checked automatically by Cyrus and will result in a 400 Bad Request if they are missing.

* Cyrus uses custom validation methods for validating Commands and Queries.

* This is an example for a validator for an AddTrackCommand, returning zero or one validation errors:

```csharp
    [Validator]
    public static IEnumerable<string> Validate(AddTrackCommand addTrack)
    {
        if (addTrack.TrackId == TrackId.Empty) yield return "TrackId is required.";
    }
```

* Validators can be static or instance methods. Instance methods support Dependency Injection. Requirements are:
    - Need to return IEnumerable<string>
    - Need to have one argument which is either a Command or Query.

* Cyrus will automatically register the classes with validation methods, so they can be used with Dependency Injection. 

* Validators are called whenever a MapPost, MapPut, MapDelete or MapGet is invoked, resulting in a BadRequest when validation errors are returned by the validator.