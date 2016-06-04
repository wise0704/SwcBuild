namespace SwcBuild.IO.Compression
{
    internal static class Messages
	{
		internal static string ArgumentNeedNonNegative => "The argument must be non-negative.";
		internal static string CannotBeEmpty => "String cannot be empty.";
		internal static string CDCorrupt => "Central Directory corrupt.";
		internal static string CentralDirectoryInvalid => "Central Directory is invalid.";
		internal static string CreateInReadMode => "Cannot create entries on an archive opened in read mode.";
		internal static string CreateModeCapabilities => "Cannot use create mode on a non-writeable stream.";
		internal static string CreateModeCreateEntryWhileOpen => "Entries cannot be created while previously created entries are still open.";
		internal static string CreateModeWriteOnceAndOneEntryAtATime => "Entries in create mode may only be written to once, and only one entry may be held open at a time.";
		internal static string DateTimeInvalid => "The DateTime in the Zip file is invalid.";
		internal static string DateTimeOutOfRange => "The DateTimeOffset specified cannot be converted into a Zip file timestamp.";
		internal static string DeletedEntry => "Cannot modify deleted entry.";
		internal static string DeleteOnlyInUpdate => "Delete can only be used when the archive is in Update mode.";
		internal static string DeleteOpenEntry => "Cannot delete an entry currently open for writing.";
		internal static string EntriesInCreateMode => "Cannot access entries in Create mode.";
		internal static string EntryNameEncodingNotSupported => "The specified entry name encoding is not supported.";
		internal static string EntryNamesTooLong => "Entry names cannot require more than 2^16 bits.";
		internal static string EntryTooLarge => "Entries larger than 4GB are not supported in Update mode.";
		internal static string EOCDNotFound => "End of Central Directory record could not be found.";
		internal static string FieldTooBigCompressedSize => "Compressed Size cannot be held in an Int64.";
		internal static string FieldTooBigLocalHeaderOffset => "Local Header Offset cannot be held in an Int64.";
		internal static string FieldTooBigNumEntries => "Number of Entries cannot be held in an Int64.";
		internal static string FieldTooBigOffsetToCD => "Offset to Central Directory cannot be held in an Int64.";
		internal static string FieldTooBigOffsetToZip64EOCD => "Offset to Zip64 End Of Central Directory record cannot be held in an Int64.";
		internal static string FieldTooBigStartDiskNumber => "Start Disk Number cannot be held in an Int64.";
		internal static string FieldTooBigUncompressedSize => "Uncompressed Size cannot be held in an Int64.";
		internal static string FrozenAfterWrite => "Cannot modify entry in Create mode after entry has been opened for writing.";
		internal static string HiddenStreamName => "A stream from ZipArchiveEntry has been disposed.";
		internal static string LengthAfterWrite => "Length properties are unavailable once an entry has been opened for writing.";
		internal static string LocalFileHeaderCorrupt => "A local file header is corrupt.";
		internal static string NumEntriesWrong => "Number of entries expected in End Of Central Directory does not correspond to number of entries in Central Directory.";
		internal static string OffsetLengthInvalid => "The offset and length parameters are not valid for the array that was given.";
		internal static string ReadingNotSupported => "This stream from ZipArchiveEntry does not support reading.";
		internal static string ReadModeCapabilities => "Cannot use read mode on a non-readable stream.";
		internal static string ReadOnlyArchive => "Cannot modify read-only archive.";
		internal static string SeekingNotSupported => "This stream from ZipArchiveEntry does not support seeking.";
		internal static string SetLengthRequiresSeekingAndWriting => "SetLength requires a stream that supports seeking and writing.";
		internal static string SplitSpanned => "Split or spanned archives are not supported.";
		internal static string UnexpectedEndOfStream => "Zip file corrupt: unexpected end of stream reached.";
		internal static string UnsupportedCompression => "The archive entry was compressed using an unsupported compression method.";
		internal static string UpdateModeCapabilities => "Update mode requires a stream with read, write, and seek capabilities.";
		internal static string UpdateModeOneStream => "Entries cannot be opened multiple times in Update mode.";
		internal static string WritingNotSupported => "This stream from ZipArchiveEntry does not support writing.";
		internal static string Zip64EOCDNotWhereExpected => "Zip 64 End of Central Directory Record not where indicated.";
	}
}
