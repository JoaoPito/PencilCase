import logging
from logging.handlers import RotatingFileHandler

def configure_logging(log_filepath, loglevel=logging.INFO):
    logging.basicConfig(
        filename=log_filepath,
        format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
        level=loglevel
    )

    log_handler = RotatingFileHandler(
        log_filepath, 
        maxBytes=1024 * 100, 
        backupCount=10
        )
    logging.getLogger().addHandler(log_handler)