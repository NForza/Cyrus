﻿using NForza.TypedIds;

namespace DemoApp.Contracts;

[StringIdAttribute(minimumLength:1, maximumLength:200)]
public partial record struct Address;
