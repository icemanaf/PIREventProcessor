﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive;
using System.Reactive.Linq;

namespace EventProcessor.EventSinks
{
    public interface IEventSink<T>
    {
        void Observe(IObservable<T> observable);

        bool Enabled();
    }
}
