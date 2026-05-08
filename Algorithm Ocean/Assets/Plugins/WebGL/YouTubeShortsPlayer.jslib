mergeInto(LibraryManager.library, {
  $AOYT_CreateBridge: function () {
    const players = {};
    let apiReady = false;
    let apiPromise = null;

    function loadApi() {
      if (apiReady) {
        return Promise.resolve();
      }

      if (apiPromise) {
        return apiPromise;
      }

      apiPromise = new Promise(function (resolve) {
        const previousReady = window.onYouTubeIframeAPIReady;
        window.onYouTubeIframeAPIReady = function () {
          apiReady = true;
          if (typeof previousReady === "function") {
            previousReady();
          }
          resolve();
        };

        if (window.YT && window.YT.Player) {
          apiReady = true;
          resolve();
          return;
        }

        const script = document.createElement("script");
        script.src = "https://www.youtube.com/iframe_api";
        document.head.appendChild(script);
      });

      return apiPromise;
    }

    function getUnityCanvas() {
      return document.querySelector("#unity-canvas") ||
        document.querySelector("canvas") ||
        document.body;
    }

    function createContainer(objectName) {
      const container = document.createElement("div");
      container.id = "ao-youtube-" + objectName;
      container.style.position = "absolute";
      container.style.overflow = "hidden";
      container.style.backgroundColor = "black";
      container.style.zIndex = "10";
      container.style.display = "none";
      container.style.pointerEvents = "auto";

      const playerElement = document.createElement("div");
      playerElement.id = container.id + "-player";
      container.appendChild(playerElement);

      document.body.appendChild(container);
      return { container, playerElement };
    }

    function getCanvasRect() {
      const canvas = getUnityCanvas();
      return canvas.getBoundingClientRect();
    }

    function sendMessage(objectName, method, value) {
      if (typeof SendMessage === "function") {
        SendMessage(objectName, method, value || "");
      }
    }

    function stateName(state) {
      switch (state) {
        case YT.PlayerState.ENDED: return "ended";
        case YT.PlayerState.PLAYING: return "playing";
        case YT.PlayerState.PAUSED: return "paused";
        case YT.PlayerState.BUFFERING: return "buffering";
        case YT.PlayerState.CUED: return "cued";
        default: return "unstarted";
      }
    }

    function ensurePlayer(objectName, options) {
      if (players[objectName]) {
        return players[objectName];
      }

      const elements = createContainer(objectName);
      const entry = {
        objectName: objectName,
        container: elements.container,
        playerElement: elements.playerElement,
        options: options,
        videoId: "",
        player: null,
        visible: false,
        rect: { x: 0, y: 0, width: 0, height: 0 }
      };

      players[objectName] = entry;
      loadApi().then(function () {
        buildPlayer(entry);
      });
      return entry;
    }

    function buildPlayer(entry) {
      if (entry.player || !window.YT || !window.YT.Player) {
        return;
      }

      entry.player = new YT.Player(entry.playerElement.id, {
        width: "100%",
        height: "100%",
        videoId: entry.videoId,
        playerVars: {
          autoplay: 0,
          controls: entry.options.controls ? 1 : 0,
          playsinline: 1,
          rel: 0,
          loop: entry.options.loop ? 1 : 0,
          playlist: entry.options.loop && entry.videoId ? entry.videoId : undefined
        },
        events: {
          onReady: function (event) {
            if (entry.options.muted) {
              event.target.mute();
            }
            sendMessage(entry.objectName, "OnYouTubePlayerReady", "");
          },
          onStateChange: function (event) {
            sendMessage(entry.objectName, "OnYouTubePlayerState", stateName(event.data));
          },
          onError: function (event) {
            sendMessage(entry.objectName, "OnYouTubePlayerError", String(event.data));
          }
        }
      });
    }

    function applyRect(entry) {
      const canvasRect = getCanvasRect();
      entry.container.style.left = (canvasRect.left + canvasRect.width * entry.rect.x) + "px";
      entry.container.style.top = (canvasRect.top + canvasRect.height * entry.rect.y) + "px";
      entry.container.style.width = (canvasRect.width * entry.rect.width) + "px";
      entry.container.style.height = (canvasRect.height * entry.rect.height) + "px";
    }

    window.addEventListener("resize", function () {
      Object.keys(players).forEach(function (key) {
        applyRect(players[key]);
      });
    });

    window.addEventListener("scroll", function () {
      Object.keys(players).forEach(function (key) {
        applyRect(players[key]);
      });
    }, true);

    return {
      create: function (objectName, options) {
        ensurePlayer(objectName, options);
      },

      loadVideo: function (objectName, videoId, autoplay) {
        const entry = ensurePlayer(objectName, { controls: true, muted: true, loop: false });
        entry.videoId = videoId;
        entry.container.style.display = entry.visible ? "block" : "none";

        loadApi().then(function () {
          buildPlayer(entry);
          if (!entry.player || !entry.player.loadVideoById) {
            return;
          }

          if (entry.options.loop) {
            entry.player.loadPlaylist({
              playlist: [videoId],
              index: 0
            });
            if (!autoplay) {
              entry.player.pauseVideo();
            }
            return;
          }

          if (autoplay) {
            entry.player.loadVideoById(videoId);
          } else {
            entry.player.cueVideoById(videoId);
          }
        });
      },

      setRect: function (objectName, x, y, width, height) {
        const entry = ensurePlayer(objectName, { controls: true, muted: true, loop: false });
        entry.rect = { x: x, y: y, width: width, height: height };
        applyRect(entry);
      },

      play: function (objectName) {
        const entry = players[objectName];
        if (entry && entry.player && entry.player.playVideo) {
          entry.player.playVideo();
        }
      },

      pause: function (objectName) {
        const entry = players[objectName];
        if (entry && entry.player && entry.player.pauseVideo) {
          entry.player.pauseVideo();
        }
      },

      stop: function (objectName) {
        const entry = players[objectName];
        if (entry && entry.player && entry.player.stopVideo) {
          entry.player.stopVideo();
        }
      },

      setVisible: function (objectName, visible) {
        const entry = players[objectName];
        if (!entry) {
          return;
        }
        entry.visible = visible;
        entry.container.style.display = visible ? "block" : "none";
        applyRect(entry);
      },

      destroy: function (objectName) {
        const entry = players[objectName];
        if (!entry) {
          return;
        }

        if (entry.player && entry.player.destroy) {
          entry.player.destroy();
        }

        entry.container.remove();
        delete players[objectName];
      }
    };
  },

  $AOYT_GetBridge__deps: ["$AOYT_CreateBridge"],
  $AOYT_GetBridge: function () {
    if (!window.AlgorithmOceanYouTube) {
      window.AlgorithmOceanYouTube = AOYT_CreateBridge();
    }
    return window.AlgorithmOceanYouTube;
  },

  AOYT_Create__deps: ["$AOYT_GetBridge"],
  AOYT_Create: function (objectNamePtr, controls, muted, loop) {
    const objectName = UTF8ToString(objectNamePtr);
    AOYT_GetBridge().create(objectName, {
      controls: controls === 1,
      muted: muted === 1,
      loop: loop === 1
    });
  },

  AOYT_LoadVideo__deps: ["$AOYT_GetBridge"],
  AOYT_LoadVideo: function (objectNamePtr, videoIdPtr, autoplay) {
    const objectName = UTF8ToString(objectNamePtr);
    const videoId = UTF8ToString(videoIdPtr);
    AOYT_GetBridge().loadVideo(objectName, videoId, autoplay === 1);
  },

  AOYT_SetRect__deps: ["$AOYT_GetBridge"],
  AOYT_SetRect: function (objectNamePtr, x, y, width, height) {
    const objectName = UTF8ToString(objectNamePtr);
    AOYT_GetBridge().setRect(objectName, x, y, width, height);
  },

  AOYT_Play__deps: ["$AOYT_GetBridge"],
  AOYT_Play: function (objectNamePtr) {
    const objectName = UTF8ToString(objectNamePtr);
    AOYT_GetBridge().play(objectName);
  },

  AOYT_Pause__deps: ["$AOYT_GetBridge"],
  AOYT_Pause: function (objectNamePtr) {
    const objectName = UTF8ToString(objectNamePtr);
    AOYT_GetBridge().pause(objectName);
  },

  AOYT_Stop__deps: ["$AOYT_GetBridge"],
  AOYT_Stop: function (objectNamePtr) {
    const objectName = UTF8ToString(objectNamePtr);
    AOYT_GetBridge().stop(objectName);
  },

  AOYT_SetVisible__deps: ["$AOYT_GetBridge"],
  AOYT_SetVisible: function (objectNamePtr, visible) {
    const objectName = UTF8ToString(objectNamePtr);
    AOYT_GetBridge().setVisible(objectName, visible === 1);
  },

  AOYT_Destroy__deps: ["$AOYT_GetBridge"],
  AOYT_Destroy: function (objectNamePtr) {
    const objectName = UTF8ToString(objectNamePtr);
    AOYT_GetBridge().destroy(objectName);
  }
});
