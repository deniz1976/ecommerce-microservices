type ReadStateListener = () => void

const readStateListeners = new Set<ReadStateListener>()

export function announceNotificationsRead() {
  readStateListeners.forEach((listener) => listener())
}

export function subscribeToNotificationReadState(listener: ReadStateListener) {
  readStateListeners.add(listener)

  return () => {
    readStateListeners.delete(listener)
  }
}
