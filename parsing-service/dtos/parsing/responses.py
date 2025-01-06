from pydantic import BaseModel

class ParseFilePostResponse(BaseModel):
    chunks: list[str]