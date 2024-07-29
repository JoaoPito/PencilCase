from fastapi.middleware.cors import CORSMiddleware
from app.api_builder import ApiBuilder
from app.generators.roadmap import RoadmapGenerator

builder = ApiBuilder()

builder.add_generator(RoadmapGenerator())

app = builder.build()

# Set all CORS enabled origins
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
    expose_headers=["*"],
)

@app.get("/")
async def get_list():
    return [{gen.name: {"description":gen.description,
                        "endpoint": gen.endpoint}} 
                        for gen in builder.generators]

#@app.get("/")
#async def redirect_root_to_docs():
#    return RedirectResponse("/docs")

if __name__ == "__main__":
    import uvicorn

    uvicorn.run(app, host="0.0.0.0", port=8000)
