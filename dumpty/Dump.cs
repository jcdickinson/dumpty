using Microsoft.Diagnostics.Runtime;
using System;

namespace Dumpty
{
    /// <summary>
    /// Represents a dump.
    /// </summary>
    public class Dump
    {
        private DataTarget _activeDump;
        /// <summary>
        /// Gets or sets the active dump.
        /// </summary>
        /// <value>
        /// The active dump.
        /// </value>
        public DataTarget ActiveDump
        {
            get { return _activeDump; }
            set
            {
                if (_activeDump != value)
                {
                    if (_activeDump != null)
                        _activeDump.Dispose();
                    _activeDump = value;
                    ActiveVersion = null;
                }
            }
        }

        private ClrInfo _activeVersion;
        /// <summary>
        /// Gets or sets the active version.
        /// </summary>
        /// <value>
        /// The active version.
        /// </value>
        public ClrInfo ActiveVersion
        {
            get
            {
                if (_activeVersion == null && _activeDump != null)
                    _activeVersion = _activeDump.ClrVersions[0];
                return _activeVersion;
            }
            set
            {
                if (_activeVersion != value)
                {
                    _heap = null;
                    _runtime = null;
                    _activeVersion = value;
                }
            }
        }

        private string _dacLocation;
        /// <summary>
        /// Gets or sets the DAC location.
        /// </summary>
        /// <value>
        /// The DAC location.
        /// </value>
        public string DacLocation
        {
            get
            {
                if (_dacLocation == null && ActiveVersion != null)
                    _dacLocation = ActiveVersion.TryDownloadDac(new SymbolNotification());
                return _dacLocation;
            }
            set
            {
                if (!string.Equals(_dacLocation, value, StringComparison.OrdinalIgnoreCase))
                {
                    _heap = null;
                    _runtime = null;
                    _dacLocation = value;
                }
            }
        }

        private ClrRuntime _runtime;
        /// <summary>
        /// Gets the runtime.
        /// </summary>
        /// <value>
        /// The runtime.
        /// </value>
        public ClrRuntime Runtime
        {
            get
            {
                if (_runtime == null) _runtime = ActiveVersion.CreateRuntime(DacLocation);
                return _runtime;
            }
        }

        private ClrHeap _heap;
        /// <summary>
        /// Gets the heap.
        /// </summary>
        /// <value>
        /// The heap.
        /// </value>
        public ClrHeap Heap
        {
            get
            {
                if (_heap == null) _heap = Runtime.GetHeap();
                return _heap;
            }
        }
    }
}
