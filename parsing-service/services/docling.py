from io import BytesIO
from docling.document_converter import DocumentConverter, PdfFormatOption
from docling.datamodel.pipeline_options import PdfPipelineOptions
from docling.datamodel.base_models import InputFormat, DocumentStream
from docling.chunking import HybridChunker
from docling.exceptions import ConversionError

class DoclingService:
    def __init__(self, artifacts_path=None, chunker_tokenizer='nomic-ai/nomic-embed-text-v1.5', chunk_size=256):
        pipeline_options = PdfPipelineOptions(artifacts_path=artifacts_path)
        self.converter = DocumentConverter(
            format_options={
                InputFormat.PDF: PdfFormatOption(pipeline_options=pipeline_options)
            }
        )
        
        self.chunker = HybridChunker(tokenizer=chunker_tokenizer, max_tokens=chunk_size)
    
    def parse_and_chunk(self, file_stream: BytesIO, file_name: str) -> list[str]:
        doc = DocumentStream(name=file_name, stream=file_stream)
        try:
            result = self.converter.convert(doc)
            return [chunk.text for chunk in list(self.chunker.chunk(result.document))]
        except ConversionError:
            raise TypeError("File type cannot be converted!")