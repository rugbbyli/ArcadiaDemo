(ns minimal.core
  (:use arcadia.core arcadia.linear))

(defn first-callback [obj role-key]
  (arcadia.core/log "Hello, Arcadia"))