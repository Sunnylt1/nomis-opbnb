// ------------------------------------------------------------------------------------------------------
// <copyright file="ValuePool.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

namespace Nomis.Utils.Contracts
{
    /// <inheritdoc cref="IValuePool{TService, TValue}"/>
    public class ValuePool<TService, TValue> :
        IValuePool<TService, TValue>
        where TService : class
        where TValue : class
    {
        private readonly IList<TValue> _values;
        private int _currentValueIndex;

        /// <summary>
        /// Initialize <see cref="ValuePool{TService, TValue}"/>.
        /// </summary>
        /// <param name="values">Array of value.</param>
        public ValuePool(
            IEnumerable<TValue> values)
        {
            _values = values.ToList();
            _currentValueIndex = 0;
        }

        /// <inheritdoc />
        public TValue GetNextValue()
        {
            lock (_values)
            {
                if (_values.Count == 0)
                {
                    return default!;
                }

                var value = _values[_currentValueIndex];
                _currentValueIndex = (_currentValueIndex + 1) % _values.Count;
                return value;
            }
        }

        /// <inheritdoc />
        public int CurrentIndex
        {
            get
            {
                lock (_values)
                {
                    return _currentValueIndex;
                }
            }
        }
    }
}