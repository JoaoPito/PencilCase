from langserve import add_routes

class Generator:
    name: str
    endpoint: str
    description: str
    chain: any

    def __init__(self, endpoint=''):
        if endpoint != '':
            self.endpoint = endpoint

    def add_route(self, app):
        add_routes(app, self.chain, path=self.endpoint)