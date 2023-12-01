using System;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

namespace MetadataBuilderExample
{
    public class MetadataBuilderExampleProgram
    {
        private const string pdbName = @"bin/Debug/net7.0/ConsoleApplication.pdb";
        private const string scriptName = @"/workspace/template-dotnet-core-cli-csharp/bin/Debug/net7.0/ConsoleApplication.cs";                                            
        private static readonly Guid s_guid = new Guid("87D4DBE1-1143-4FAD-AAB3-1001F92068E6");
        private static readonly BlobContentId s_contentId = new BlobContentId(s_guid, 0x04030201);
        private static readonly Guid s_pdb_guid = new Guid("afddd956-9531-4dcb-a8ce-4ff57272afa0");
        private static readonly BlobContentId s_pdb_contentId = new BlobContentId(s_pdb_guid, 0x04030201);
        private static readonly String dllName = @"bin/Debug/net7.0/ConsoleApplication.dll";

private static MethodDefinitionHandle EmitHelloWorld(MetadataBuilder metadata, BlobBuilder ilBuilder, FileStream peStream)
{


    // Create module and assembly for a console application.
    metadata.AddModule(
        0,
        metadata.GetOrAddString(dllName),
        metadata.GetOrAddGuid(s_guid),
        default(GuidHandle),
        default(GuidHandle));

    metadata.AddAssembly(
        metadata.GetOrAddString("ConsoleApplication"),
        version: new Version(1, 0, 0, 0),
        culture: default(StringHandle),
        publicKey: default(BlobHandle),
        flags: 0,
        hashAlgorithm: AssemblyHashAlgorithm.None);
    

    // Create references to System.Object and System.Console types.
/*
    AssemblyReferenceHandle mscorlibAssemblyRef = metadata.AddAssemblyReference(
        name: metadata.GetOrAddString("mscorlib"),
        version: new Version(5, 0, 0, 0),
        culture: default(StringHandle),
        publicKeyOrToken: metadata.GetOrAddBlob(
            //new byte[] { 0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x89 }
            // .publickeytoken = (B0 3F 5F 7F 11 D5 0A 3A )   
            new byte[] { 0xB0, 0x3F, 0x5F, 0x7F, 0x11, 0xD5, 0x0A, 0x3A }
            ),
        flags: default(AssemblyFlags),
        hashValue: default(BlobHandle));
*/
    AssemblyReferenceHandle sysRuntimeAssemblyRef = metadata.AddAssemblyReference(
        name: metadata.GetOrAddString("System.Runtime"),
        version: new Version(5, 0, 0, 0),
        culture: default(StringHandle),
        publicKeyOrToken: metadata.GetOrAddBlob(
            //new byte[] { 0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x89 }
            // .publickeytoken = (B0 3F 5F 7F 11 D5 0A 3A )   
            new byte[] { 0xB0, 0x3F, 0x5F, 0x7F, 0x11, 0xD5, 0x0A, 0x3A }
            ),
        flags: default(AssemblyFlags),
        hashValue: default(BlobHandle));
            
    AssemblyReferenceHandle sysConsoleAssemblyRef = metadata.AddAssemblyReference(
        name: metadata.GetOrAddString("System.Console"),
        version: new Version(5, 0, 0, 0),
        culture: default(StringHandle),
        publicKeyOrToken: metadata.GetOrAddBlob(
            //new byte[] { 0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x89 }
            // .publickeytoken = (B0 3F 5F 7F 11 D5 0A 3A )   
            new byte[] { 0xB0, 0x3F, 0x5F, 0x7F, 0x11, 0xD5, 0x0A, 0x3A }
            ),
        flags: default(AssemblyFlags),
        hashValue: default(BlobHandle));


    TypeReferenceHandle systemObjectTypeRef = metadata.AddTypeReference(
        sysRuntimeAssemblyRef,
        metadata.GetOrAddString("System"),
        metadata.GetOrAddString("Object"));

    TypeReferenceHandle systemConsoleTypeRefHandle = metadata.AddTypeReference(
        sysConsoleAssemblyRef,
        metadata.GetOrAddString("System"),
        metadata.GetOrAddString("Console"));

    // Get reference to Console.WriteLine(string) method.
    var consoleWriteLineSignature = new BlobBuilder();

    new BlobEncoder(consoleWriteLineSignature).
        MethodSignature().
        Parameters(1,
            returnType => returnType.Void(),
            parameters => parameters.AddParameter().Type().String());

    MemberReferenceHandle consoleWriteLineMemberRef = metadata.AddMemberReference(
        systemConsoleTypeRefHandle,
        metadata.GetOrAddString("WriteLine"),
        metadata.GetOrAddBlob(consoleWriteLineSignature));

  

/*    // Get reference to Object's constructor.
    var parameterlessCtorSignature = new BlobBuilder();

    new BlobEncoder(parameterlessCtorSignature).
        MethodSignature(isInstanceMethod: true).
        Parameters(0, returnType => returnType.Void(), parameters => { });

    BlobHandle parameterlessCtorBlobIndex = metadata.GetOrAddBlob(parameterlessCtorSignature);

    MemberReferenceHandle objectCtorMemberRef = metadata.AddMemberReference(
        systemObjectTypeRef,
        metadata.GetOrAddString(".ctor"),
        parameterlessCtorBlobIndex);
*/
    // Create signature for "void Main()" method.
    var mainSignature = new BlobBuilder();

    new BlobEncoder(mainSignature).
        MethodSignature().
        Parameters(0, returnType => returnType.Void(), parameters => { });

    var methodBodyStream = new MethodBodyStreamEncoder(ilBuilder);

    var codeBuilder = new BlobBuilder();
    InstructionEncoder il;
/*    
    // Emit IL for Program::.ctor
    il = new InstructionEncoder(codeBuilder);

    // ldarg.0
    il.LoadArgument(0); 

    // call instance void [mscorlib]System.Object::.ctor()
    il.Call(objectCtorMemberRef);

    // ret
    il.OpCode(ILOpCode.Ret);

    int ctorBodyOffset = methodBodyStream.AddMethodBody(il);
    codeBuilder.Clear();
*/
    // Emit IL for Program::Main
    var flowBuilder = new ControlFlowBuilder();
    il = new InstructionEncoder(codeBuilder, flowBuilder);

    il.OpCode(ILOpCode.Nop);
    // ldstr "hello"
    var callOffset = il.Offset;    
    il.LoadString(metadata.GetOrAddUserString("Hello World"));


    // call void [mscorlib]System.Console::WriteLine(string)
    il.Call(consoleWriteLineMemberRef);
    il.OpCode(ILOpCode.Nop);
    // ret
    il.OpCode(ILOpCode.Ret);


   int mainBodyOffset = methodBodyStream.AddMethodBody(il);

 


    codeBuilder.Clear();

    // Create method definition for Program::Main
    MethodDefinitionHandle mainMethodDef = metadata.AddMethodDefinition(
        MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
        MethodImplAttributes.IL,
        metadata.GetOrAddString("Main"),
        metadata.GetOrAddBlob(mainSignature),
        mainBodyOffset,
        parameterList: default(ParameterHandle));

   // DEBUG:
  /*  metadata.AddLocalScope(
        method: mainMethodDef,
        importScope: importScope,
        variableList: NextHandle(lastLocalVariableHandle),
        constantList: NextHandle(lastLocalConstantHandle),
        startOffset: 0,
        length: methodBody.GetILReader().Length);        
    */
/*
    // Create method definition for Program::.ctor
    MethodDefinitionHandle ctorDef = metadata.AddMethodDefinition(
        MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
        MethodImplAttributes.IL,
        metadata.GetOrAddString(".ctor"),
        parameterlessCtorBlobIndex,
        ctorBodyOffset,
        parameterList: default(ParameterHandle));
*/
    // Create type definition for the special <Module> type that holds global functions
    metadata.AddTypeDefinition(
        default(TypeAttributes),
        default(StringHandle),
        metadata.GetOrAddString("<Module>"),
        baseType: default(EntityHandle),
        fieldList: MetadataTokens.FieldDefinitionHandle(1),
        methodList: mainMethodDef);

    // Create type definition for ConsoleApplication.Program
    metadata.AddTypeDefinition(
        TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.AutoLayout | TypeAttributes.BeforeFieldInit,
        metadata.GetOrAddString("ConsoleApplication"),
        metadata.GetOrAddString("Program"),
        baseType: systemObjectTypeRef,
        fieldList: MetadataTokens.FieldDefinitionHandle(1),
        methodList: mainMethodDef);

   // tables counts without DEBUG tables

    var typeSystemRowCounts = metadata.GetRowCounts();


    WritePEImage(peStream, metadata, ilBuilder, mainMethodDef);

//-------------------------------------------------------------------------------------
//  DEBUG
//-------------------------------------------------------------------------------------

   MetadataBuilder metadataDebug = new MetadataBuilder();
 
   // create pdb document
   // a. set capacity for the document table
   metadataDebug.SetCapacity(TableIndex.Document, 1);
   // b. add pdb document including absolute path
   var documentHandle = metadataDebug.AddDocument(
                name: metadataDebug.GetOrAddDocumentName(scriptName),
                hashAlgorithm: default(GuidHandle),
                hash: default(BlobHandle),
                language: default(GuidHandle));        

    // METHODS:
    // .ctor not now
    // void Main()
    metadataDebug.SetCapacity(TableIndex.MethodDebugInformation, 1);  





    
    // DEBUG sequence points
    // Blob ::= header SequencePointRecord (SequencePointRecord | document-record)*
    // SequencePointRecord ::= sequence-point-record | hidden-sequence-point-record
    var writer = new BlobBuilder();

    // header
    writer.WriteCompressedInteger(0);
    // nop
    writer.WriteCompressedInteger(0);
    writer.WriteCompressedInteger(0);
    writer.WriteCompressedInteger(10-9);
    writer.WriteCompressedInteger(8);
    writer.WriteCompressedInteger(9);

    // SequencePointRecord  
        // sequence-point-record    
            // write IL offset
            // δILOffset 	ILOffset if this is the first sequence point 	unsigned compressed
            // 	            ILOffset - Previous.ILOffset otherwise 	unsigned compressed, non-zero    
    writer.WriteCompressedInteger(callOffset);

            // ΔLines 	       EndLine - StartLine 	            unsigned compressed
            // ΔColumns 	   EndColumn - StartColumn 	    ΔLines = 0: unsigned compressed, non-zero
            // 		                                        ΔLines > 0: signed compressed
            // δStartLine 	   StartLine if this is the first non-hidden sequence point 	unsigned compressed
            // 	               StartLine - PreviousNonHidden.StartLine otherwise 	signed compressed
            // δStartColumn    StartColumn if this is the first non-hidden sequence point 	unsigned compressed
            // 	               StartColumn - PreviousNonHidden.StartColumn otherwise 	signed compressed
    writer.WriteCompressedInteger(0);
    writer.WriteCompressedInteger(47 - 13);
    writer.WriteCompressedSignedInteger(1);
    writer.WriteCompressedSignedInteger(13-9);
    // nop
    writer.WriteCompressedInteger(0x0b);
    writer.WriteCompressedInteger(0);
    writer.WriteCompressedInteger(10-9);
    writer.WriteCompressedSignedInteger(1);
    writer.WriteCompressedSignedInteger(9-13);  //writer.WriteCompressedInteger(0x79);    

    // document-record
    //writer.WriteCompressedInteger(MetadataTokens.GetRowNumber(documentHandle));


   BlobHandle sequencePointsBlob = metadataDebug.GetOrAddBlob(writer);
    

    metadataDebug.AddMethodDebugInformation(
        document: documentHandle,
        sequencePoints: sequencePointsBlob);
    metadataDebug.AddMethodDebugInformation(default(DocumentHandle), default(BlobHandle));
    



    var serializer = new PortablePdbBuilder(metadataDebug, typeSystemRowCounts, mainMethodDef, content => s_pdb_contentId);
    BlobBuilder blobBuilder = new BlobBuilder();
    serializer.Serialize(blobBuilder);
    
    var targetPdbStream = new MemoryStream();            
    blobBuilder.WriteContentTo(targetPdbStream);   

    // Create the file once we know we have successfully converted the PDB:
    
    using (var dstFileStream = new FileStream(pdbName, FileMode.Create, FileAccess.ReadWrite))
    {
        targetPdbStream.Position = 0;
        targetPdbStream.CopyTo(dstFileStream);
    }                 
    
    return mainMethodDef;
}

private static void WritePEImage(
    Stream peStream,
    MetadataBuilder metadataBuilder,
    BlobBuilder ilBuilder,
    MethodDefinitionHandle entryPointHandle
    )
{
    // Create executable with the managed metadata from the specified MetadataBuilder.
    var peHeaderBuilder = new PEHeaderBuilder(
        //machine: Machine.Amd64,
        imageCharacteristics: Characteristics.ExecutableImage
        );
    var debugDirectoryBuilder = new DebugDirectoryBuilder();
    debugDirectoryBuilder.AddCodeViewEntry(pdbName, s_pdb_contentId, 0x0100);

    var peBuilder = new ManagedPEBuilder(
        peHeaderBuilder,
        new MetadataRootBuilder(metadataBuilder),
        ilBuilder,
        entryPoint: entryPointHandle,
        debugDirectoryBuilder: debugDirectoryBuilder,
        flags: CorFlags.ILOnly | CorFlags.TrackDebugData,
        deterministicIdProvider: content => s_contentId);

    // Write executable into the specified stream.
    var peBlob = new BlobBuilder();
    BlobContentId contentId = peBuilder.Serialize(peBlob);
    peBlob.WriteContentTo(peStream);
}

public static void BuildHelloWorldApp()
{
    using var peStream = new FileStream(
        dllName, FileMode.OpenOrCreate, FileAccess.ReadWrite
        );
    
    var ilBuilder = new BlobBuilder();
    var metadataBuilder = new MetadataBuilder();

    MethodDefinitionHandle entryPoint = EmitHelloWorld(metadataBuilder, ilBuilder, peStream);

}
    }
}
