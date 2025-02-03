from services.docling import DoclingService
from dtos.parsing.requests import ParseFilePostRequest
from dtos.parsing.responses import ParseFilePostResponse
from fastapi import HTTPException

import base64
from io import BytesIO

def add_v1_parsing_endpoints(router, docling_service: DoclingService):
    @router.post('/file')
    async def parse_file(request: ParseFilePostRequest):
        stream = base64_to_stream(request.file_contents)
        return try_parse_file(stream, request.filename, docling_service)
    
def try_parse_file(stream: BytesIO, file_name:str, docling_service: DoclingService):
    try:
        return ParseFilePostResponse(
            chunks=docling_service.parse_and_chunk(stream, file_name)
            )
    except TypeError as e:
        raise HTTPException(400, "Cannot convert this type of file!")
    
def base64_to_stream(base64_str: str) -> BytesIO:
    base64_str = base64_str.strip()
    binary_data = base64.b64decode(base64_str)
    return BytesIO(binary_data)