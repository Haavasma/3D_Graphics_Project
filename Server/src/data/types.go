package data

import (
	"net"
	"sync"
)

// SafeChannelAddressSlice Thread-safe channel list
type SafeChannelAddressSlice struct {
	V   map[string][]*net.UDPAddr
	Mux sync.Mutex
}

// SafeQueue safe queue containing addresses
type SafeQueue struct {
	Q   []net.Conn
	Mux sync.Mutex
}
