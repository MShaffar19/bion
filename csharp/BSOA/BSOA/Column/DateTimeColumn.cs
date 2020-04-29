﻿using BSOA.IO;
using System;
using System.Collections;
using System.Collections.Generic;

namespace BSOA.Column
{
    /// <summary>
    ///  DateTimeColumn implements IColumn for DateTime on top of a NumberColumn&lt;long&gt;
    /// </summary>
    public class DateTimeColumn : IColumn<DateTime>
    {
        private NumberColumn<long> _inner;

        public DateTimeColumn(DateTime defaultValue)
        {
            _inner = new NumberColumn<long>(defaultValue.ToUniversalTime().Ticks);
        }

        public int Count => _inner.Count;
        
        public bool Empty => Count == 0;
        
        public DateTime this[int index]
        {
            get { return new DateTime(_inner[index], DateTimeKind.Utc); }
            set { _inner[index] = value.ToUniversalTime().Ticks; }
        }

        public void Clear()
        {
            _inner.Clear();
        }

        public void Trim()
        {
            _inner.Trim();
        }

        public IEnumerator<DateTime> GetEnumerator()
        {
            return new ListEnumerator<DateTime>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ListEnumerator<DateTime>(this);
        }

        public void Read(ITreeReader reader)
        {
            _inner.Read(reader);
        }

        public void Write(ITreeWriter writer)
        {
            _inner.Write(writer);
        }
    }
}
