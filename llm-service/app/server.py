from fastapi.middleware.cors import CORSMiddleware
from app.api_builder import ApiBuilder
from app.generators.roadmap import RoadmapGenerator
from app.generators.introduction import IntroductionGenerator
from app.generators.applications import ApplicationsGenerator

builder = ApiBuilder()

builder.add_generator(IntroductionGenerator())
builder.add_generator(RoadmapGenerator())
builder.add_generator(ApplicationsGenerator())

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

if __name__ == "__main__":
    import uvicorn

    uvicorn.run(app, host="0.0.0.0", port=80)
