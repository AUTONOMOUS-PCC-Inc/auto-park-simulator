from http.server import SimpleHTTPRequestHandler
from socketserver import ThreadingMixIn
from http.server import HTTPServer
import argparse

class ThreadingHTTPServer(ThreadingMixIn, HTTPServer):
    daemon_threads = True  # 메인 종료되면 스레드도 같이 종료

class GzipHandler(SimpleHTTPRequestHandler):
    def end_headers(self):
        if self.path.endswith(".gz"):
            self.send_header("Content-Encoding", "gzip")
            if self.path.endswith(".js.gz"):
                self.send_header("Content-Type", "application/javascript")
            elif self.path.endswith(".wasm.gz"):
                self.send_header("Content-Type", "application/wasm")
            elif self.path.endswith(".data.gz"):
                self.send_header("Content-Type", "application/octet-stream")
        super().end_headers()

if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--port", type=int, default=8080)
    args = parser.parse_args()

    ThreadingHTTPServer(("", args.port), GzipHandler).serve_forever()