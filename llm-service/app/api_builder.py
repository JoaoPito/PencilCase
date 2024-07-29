from fastapi import FastAPI
from fastapi.responses import RedirectResponse
from app.generators.generator import Generator

class ApiBuilder:
    generators: list

    def __init__(self):
        self.generators = []
        pass

    def add_generator(self, generator: Generator):
        self.generators.append(generator)

    def __add_generators_routes__(self, app):
        for generator in self.generators:
            generator.add_route(app)

    def __add_generators_list_endpoint__(self, app):
        @app.get("/")
        async def get_list():
            return [{gen.name: {"description":gen.description,
                                "endpoint": gen.endpoint}} 
                                for gen in self.generators]
                        
    def build(self):
        app = FastAPI()
        self.__add_generators_routes__(app)
        self.__add_generators_list_endpoint__(app)
        return app