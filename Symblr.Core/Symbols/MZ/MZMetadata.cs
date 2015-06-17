using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Symblr.IO;
using System.Runtime.InteropServices;
using System.Globalization;

namespace Symblr.Symbols.MZ
{
    /// <summary>
    /// Represents metadata about an MZ file.
    /// </summary>
    sealed class MZMetadata : ISymbolMetadata
    {
        private static readonly byte[] MZHeader = new byte[] { 0x4D, 0x5A };
        private static readonly byte[] PEHeader = new byte[] { 0x50, 0x45, 0x00, 0x00 };

        /// <summary>
        /// Gets a value indicating whether the symbols have source server information.
        /// </summary>
        /// <value>
        /// <c>true</c> if the symbols have source server information; otherwise, <c>false</c>.
        /// </value>
        public bool HasSourceServerInformation
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether information is supported.
        /// </summary>
        /// <value>
        /// <c>true</c> if source information is supported; otherwise, <c>false</c>.
        /// </value>
        public bool SupportsSourceServerInformation
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the source information from the metadata.
        /// </summary>
        /// <exception cref="System.NotSupportedException">Thrown if an attempt is made to set the value.</exception>
        public SourceInformationCollection SourceInformation
        {
            get { return SourceInformationCollection.Empty; }
            set { throw new NotSupportedException(Resources.NotSupportedException_SourceInformation); }
        }
        /// <summary>
        /// Gets the identifier of the symbol.
        /// </summary>
        /// <value>
        /// The identifier of the symbol.
        /// </value>
        public string Identifier
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MZMetadata"/> class.
        /// </summary>
        private MZMetadata(string identifier)
        {
            Identifier = identifier;
        }

        /// <summary>
        /// Asynchronously attempts to open the specified <see cref="Stream"/> as an MZ file.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <returns>A <see cref="Task{MzMetadata}"/> that represents the asynchronous open operation.</returns>
        public static async Task<ISymbolMetadata> TryOpenAsync(Stream stream, CancellationToken cancellationToken)
        {
            using (var reader = new AsyncBinaryReader(stream))
            {
                try
                {
                    // ------------- IMAGE_DOS_HEADER -------------
                    //  WORD   e_magic='MZ';                        // 0
                    //  WORD   e_cblp;                              // 2 
                    //  WORD   e_cp;                                // 4
                    //  WORD   e_crlc;                              // 6
                    //  WORD   e_cparhdr;                           // 8
                    //  WORD   e_minalloc;                          // 10
                    //  WORD   e_maxalloc;                          // 12
                    //  WORD   e_ss;                                // 14
                    //  WORD   e_sp;                                // 16
                    //  WORD   e_csum;                              // 18
                    //  WORD   e_ip;                                // 20
                    //  WORD   e_cs;                                // 22
                    //  WORD   e_lfarlc;                            // 24
                    //  WORD   e_ovno;                              // 26
                    //  WORD   e_res[4];                            // 28
                    //  WORD   e_oemid;                             // 36
                    //  WORD   e_oeminfo;                           // 38
                    //  WORD   e_res2[10];                          // 40
                    //  LONG   e_lfanew;                            // 60
                    // ------------- (DOS_STUB) -------------
                    // ...
                    // ------------- PE SIGNATURE -------------
                    //  DWORD  Signature;                           // e_lfanew + 0
                    // ------------- IMAGE_FILE_HEADER -------------
                    //  WORD   Machine={0x014c,0x0200,0x8664};      // e_lfanew + 4
                    //  WORD   NumberOfSections;                    // e_lfanew + 6
                    //  DWORD  TimeDateStamp;                       // e_lfanew + 8
                    //  DWORD  PointerToSymbolTable;                // e_lfanew + 12
                    //  DWORD  NumberOfSymbols;                     // e_lfanew + 16
                    //  WORD   SizeOfOptionalHeader;                // e_lfanew + 20
                    //  WORD   Characteristics;                     // e_lfanew + 22
                    // ------------- IMAGE_OPTIONAL_HEADER32 -------------
                    //  WORD    Magic=0x10b;                        // e_lfanew + 24
                    //  BYTE    MajorLinkerVersion;                 // e_lfanew + 26
                    //  BYTE    MinorLinkerVersion;                 // e_lfanew + 27
                    //  DWORD   SizeOfCode;                         // e_lfanew + 28
                    //  DWORD   SizeOfInitializedData;              // e_lfanew + 32
                    //  DWORD   SizeOfUninitializedData;            // e_lfanew + 36
                    //  DWORD   AddressOfEntryPoint;                // e_lfanew + 40
                    //  DWORD   BaseOfCode;                         // e_lfanew + 44
                    //  DWORD   BaseOfData;                         // e_lfanew + 48
                    //  DWORD   ImageBase;                          // e_lfanew + 52
                    //  DWORD   SectionAlignment;                   // e_lfanew + 56
                    //  DWORD   FileAlignment;                      // e_lfanew + 60
                    //  WORD    MajorOperatingSystemVersion;        // e_lfanew + 64
                    //  WORD    MinorOperatingSystemVersion;        // e_lfanew + 66
                    //  WORD    MajorImageVersion;                  // e_lfanew + 68
                    //  WORD    MinorImageVersion;                  // e_lfanew + 70
                    //  WORD    MajorSubsystemVersion;              // e_lfanew + 72
                    //  WORD    MinorSubsystemVersion;              // e_lfanew + 74
                    //  DWORD   Win32VersionValue;                  // e_lfanew + 76
                    //  DWORD   SizeOfImage;                        // e_lfanew + 80
                    //  DWORD   SizeOfHeaders;                      // e_lfanew + 84
                    //  DWORD   CheckSum;                           // e_lfanew + 88
                    //  WORD    Subsystem;                          // e_lfanew + 92
                    //  WORD    DllCharacteristics;                 // e_lfanew + 94
                    //  DWORD   SizeOfStackReserve;                 // e_lfanew + 96
                    //  DWORD   SizeOfStackCommit;                  // e_lfanew + 100
                    //  DWORD   SizeOfHeapReserve;                  // e_lfanew + 104
                    //  DWORD   SizeOfHeapCommit;                   // e_lfanew + 108
                    //  DWORD   LoaderFlags;                        // e_lfanew + 112
                    //  DWORD   NumberOfRvaAndSizes;                // e_lfanew + 116
                    // ------------- IMAGE_OPTIONAL_HEADER64 -------------
                    //  WORD        Magic=0x20b;                    // e_lfanew + 24
                    //  BYTE        MajorLinkerVersion;             // e_lfanew + 26
                    //  BYTE        MinorLinkerVersion;             // e_lfanew + 27
                    //  DWORD       SizeOfCode;                     // e_lfanew + 28
                    //  DWORD       SizeOfInitializedData;          // e_lfanew + 32
                    //  DWORD       SizeOfUninitializedData;        // e_lfanew + 36
                    //  DWORD       AddressOfEntryPoint;            // e_lfanew + 40
                    //  DWORD       BaseOfCode;                     // e_lfanew + 44
                    //  ULONGLONG   ImageBase;                      // e_lfanew + 48
                    //  DWORD       SectionAlignment;               // e_lfanew + 56
                    //  DWORD       FileAlignment;                  // e_lfanew + 60
                    //  WORD        MajorOperatingSystemVersion;    // e_lfanew + 64
                    //  WORD        MinorOperatingSystemVersion;    // e_lfanew + 66
                    //  WORD        MajorImageVersion;              // e_lfanew + 68
                    //  WORD        MinorImageVersion;              // e_lfanew + 70
                    //  WORD        MajorSubsystemVersion;          // e_lfanew + 72
                    //  WORD        MinorSubsystemVersion;          // e_lfanew + 74
                    //  DWORD       Win32VersionValue;              // e_lfanew + 76
                    //  DWORD       SizeOfImage;                    // e_lfanew + 80
                    //  DWORD       SizeOfHeaders;                  // e_lfanew + 84
                    //  DWORD       CheckSum;                       // e_lfanew + 88
                    //  WORD        Subsystem;                      // e_lfanew + 92
                    //  WORD        DllCharacteristics;             // e_lfanew + 94
                    //  ULONGLONG   SizeOfStackReserve;             // e_lfanew + 96
                    //  ULONGLONG   SizeOfStackCommit;              // e_lfanew + 104
                    //  ULONGLONG   SizeOfHeapReserve;              // e_lfanew + 112
                    //  ULONGLONG   SizeOfHeapCommit;               // e_lfanew + 120
                    //  DWORD       LoaderFlags;                    // e_lfanew + 128
                    //  DWORD       NumberOfRvaAndSizes;            // e_lfanew + 132

                    var IMAGE_DOS_HEADER_e_magic = await reader.ReadBytesAsync(MZHeader.Length, cancellationToken);
                    if (!NativeMethods.MemoryEquals(IMAGE_DOS_HEADER_e_magic, MZHeader)) return null;
                    
                    stream.Position = 60;
                    var IMAGE_DOS_HEADER_e_lfanew = await reader.ReadUInt32Async(cancellationToken);

                    stream.Position = IMAGE_DOS_HEADER_e_lfanew;
                    var PE_Signature = await reader.ReadBytesAsync(PEHeader.Length, cancellationToken);
                    if (!NativeMethods.MemoryEquals(PE_Signature, PEHeader)) return null;

                    stream.Position = IMAGE_DOS_HEADER_e_lfanew + 4;
                    var IMAGE_FILE_HEADER_Machine = await reader.ReadUInt16Async(cancellationToken);

                    if (IMAGE_FILE_HEADER_Machine != 0x014c &&
                        IMAGE_FILE_HEADER_Machine != 0x0200 &&
                        IMAGE_FILE_HEADER_Machine != 0x8664)
                        return null;

                    stream.Position = IMAGE_DOS_HEADER_e_lfanew + 8;
                    var IMAGE_FILE_HEADER_TimeDateStamp = await reader.ReadUInt32Async(cancellationToken);

                    stream.Position = IMAGE_DOS_HEADER_e_lfanew + 24;
                    var IMAGE_OPTIONAL_HEADER_Magic = await reader.ReadUInt16Async(cancellationToken);

                    if (IMAGE_OPTIONAL_HEADER_Magic != 0x10b &&
                        IMAGE_OPTIONAL_HEADER_Magic != 0x20b)
                        return null;

                    stream.Position = IMAGE_DOS_HEADER_e_lfanew + 80;
                    var IMAGE_OPTIONAL_HEADER_SizeOfImage = await reader.ReadUInt16Async(cancellationToken);

                    return new MZMetadata(
                        string.Format(CultureInfo.InvariantCulture, "{0:X8}{1:x4}", IMAGE_FILE_HEADER_TimeDateStamp, IMAGE_OPTIONAL_HEADER_SizeOfImage)
                        );
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Asyncronously saves any changes made to the metadata.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Task" /> that represents the pending save operation.
        /// </returns>
        /// <exception cref="System.NotSupportedException">Always thrown.</exception>
        public Task SaveAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotSupportedException(Resources.NotSupportedException_SourceInformation);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {

        }
    }
}
