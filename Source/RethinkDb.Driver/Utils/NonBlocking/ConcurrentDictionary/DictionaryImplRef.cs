﻿// Copyright (c) Vladimir Sadov. All rights reserved.
//
// This file is distributed under the MIT License. See LICENSE.md for details.

using System.Threading;
using RethinkDb.Driver.Utils.NonBlocking.Counter;

namespace RethinkDb.Driver.Utils.NonBlocking.ConcurrentDictionary
{
    internal sealed class DictionaryImplRef<TKey, TKeyStore, TValue>
            : DictionaryImpl<TKey, TKey, TValue>
                    where TKey : class
    {
        internal DictionaryImplRef(int capacity, ConcurrentDictionary<TKey, TValue> topDict)
            : base(capacity, topDict)
        {
        }

        internal DictionaryImplRef(int capacity, DictionaryImplRef<TKey, TKeyStore, TValue> other)
            : base(capacity, other)
        {
        }

        protected override bool TryClaimSlotForPut(ref TKey entryKey, TKey key, Counter32 slots)
        {
            return TryClaimSlot(ref entryKey, key, slots);
        }

        protected override bool TryClaimSlotForCopy(ref TKey entryKey, TKey key, Counter32 slots)
        {
            return TryClaimSlot(ref entryKey, key, slots);
        }

        private bool TryClaimSlot(ref TKey entryKey, TKey key, Counter32 slots)
        {
            var entryKeyValue = entryKey;
            if (entryKeyValue == null)
            {
                entryKeyValue = Interlocked.CompareExchange(ref entryKey, key, null);
                if (entryKeyValue == null)
                {
                    // claimed a new slot
                    slots.Increment();
                    return true;
                }
            }

            return key == entryKeyValue || _keyComparer.Equals(key, entryKeyValue);
        }

        protected override bool keyEqual(TKey key, TKey entryKey)
        {
            return key == entryKey || _keyComparer.Equals(key, entryKey);
        }

        protected override DictionaryImpl<TKey, TKey, TValue> CreateNew(int capacity)
        {
            return new DictionaryImplRef<TKey, TKeyStore, TValue>(capacity, this);
        }

        protected override TKey keyFromEntry(TKey entryKey)
        {
            return entryKey;
        }
    }
}