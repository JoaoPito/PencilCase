from pydantic import BaseModel

class ParseFilePostRequest(BaseModel):
    filename: str
    file_contents: str