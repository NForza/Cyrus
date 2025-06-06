﻿using NForza.Cyrus.Abstractions;

namespace TracksDemo;

public class MyCyrusConfiguration : CyrusConfig
{
    public MyCyrusConfiguration()
    {
        UseMassTransit();
        UseEntityFrameworkPersistence<global::TracksDemo.DemoContext>();
    }
}
