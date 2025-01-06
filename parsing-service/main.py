from fastapi import FastAPI, APIRouter
from dotenv import load_dotenv
import os
from logger.logger import configure_logging
from services.docling import DoclingService
from endpoints.parsing import add_v1_parsing_endpoints
import uvicorn

load_dotenv()

# Configuration
debug = os.getenv("DEBUG_MODE", 0) == 1
host_name = os.getenv("API_HOSTNAME", "localhost")
host_port = os.getenv("API_PORT", 8003)

log_filepath = os.getenv("LOG_FILEPATH", "./app.log")
configure_logging(log_filepath)

docling_artifacts_path = os.getenv("DOCLING_ARTIFACTS_PATH", None)
docling_chunk_size = os.getenv("DOCLING_CHUNK_SIZE", 1024)
docling_service = DoclingService(
    artifacts_path=docling_artifacts_path,
    chunk_size=docling_chunk_size
    )

#Routing
v1_router = APIRouter(prefix='/v1')

add_v1_parsing_endpoints(v1_router, docling_service)

if __name__=="__main__":
    uvicorn.run("main:app", host=host_name, port=host_port , reload=debug)